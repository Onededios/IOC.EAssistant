import asyncio
from playwright.async_api import async_playwright
from urllib.parse import urljoin, urlparse
import os
import json
from loki_logger import LokiLogger

BASE_URL = "https://ioc.xtec.cat/educacio/"

class WebCrawler:
    def __init__(self, start_url):
        self.start_url = start_url
        self.urls_pool = set()
        self.urls_visited = set()
        self.playwright = None
        self.browser = None
        self.page = None
        self.logger = LokiLogger("https://jiongzexu52.grafana.net", tenant_id="tenant1")


    async def close(self):
        """ Close browser and playwright instances """
        if self.page:
            await self.page.close()
        if self.browser:
            await self.browser.close()
        if self.playwright:
            await self.playwright.stop()
        try:
            self.logger.send_log(
                message="Browser closed",
                labels={"job": "web_crawler", "event": "browser_closed"}
            )
        except Exception as log_exc:
            print(f"Logging failed during close: {log_exc}")
    
    async def init_browser(self):
        """ Initialize Playwright browser and page """
        self.logger.send_log(
            message="Initializing browser",
            labels={"job": "web_crawler", "event": "browser_init"}
        )
        
        self.playwright = await async_playwright().start()
        self.browser = await self.playwright.chromium.launch(headless=False)
        self.page = await self.browser.new_page()

        try:
            await self.page.goto(self.start_url)
            await self.page.wait_for_load_state('networkidle')

            if self.page.url != self.start_url:
                async def wait_for_start_url(timeout_url, check_interval=1):
                    while self.page.url != timeout_url:
                        await asyncio.sleep(check_interval)
                try:
                    # Wait up to 120 seconds for navigation back to start_url
                    await asyncio.wait_for(wait_for_start_url(self.start_url), timeout=120)
                    print("Captcha solved, continuing...")
                    self.logger.send_log(
                        message="Captcha solved successfully",
                        labels={"job": "web_crawler", "event": "captcha_solved"}
                    )
                except asyncio.TimeoutError:
                    error_msg = f"Timeout waiting for navigation back to {self.start_url} after captcha."
                    print(error_msg)
                    self.logger.send_log(
                        message=error_msg,
                        labels={"job": "web_crawler", "event": "captcha_timeout", "level": "error"}
                    )
                    await self.close()
                    raise RuntimeError(error_msg)
            
        except Exception as e:
            print(f"Error initializing browser: {e}")
            try:
                self.logger.send_log(
                    message=f"Error initializing browser: {str(e)}",
                    labels={"job": "web_crawler", "event": "browser_init_error", "level": "error"},
                    metadata={"error": str(e)}
                )
            except Exception as log_exc:
                print(f"Logging failed: {log_exc}")
            finally:
                await self.close()
            raise

    async def parse_url(self, link, base_domain):
        """ Parse and normalize URL """
        href = await link.get_attribute('href')
        if href:
            # Skip hash links and empty hrefs
            if '#' in href or href.strip() == '':
                return None
            
            # Convert relative URLs to absolute URLs
            absolute_url = urljoin(self.start_url, href)
            parsed_url = urlparse(absolute_url)
            
            # Only include internal links (same domain)
            if parsed_url.netloc == base_domain or parsed_url.netloc == '':
                return absolute_url
        return None

    async def get_general_urls(self):
        """
        Extract general URLs from the home page's navbar.
        Returns internal links found in #header, excluding # tags.
        """
        self.logger.send_log(
            message="Starting extraction of general URLs from navbar",
            labels={"job": "web_crawler", "event": "extract_general_urls"}
        )
        
        # Find all <a> tags within the header
        navbar_links = await self.page.query_selector_all('.nav a[href]')
        
        general_urls = []
        base_domain = urlparse(self.start_url).netloc
        
        for link in navbar_links:
            url = await self.parse_url(link, base_domain)
            if url:
                general_urls.append(url)
        
        # Remove duplicates while preserving order
        unique_urls = list(dict.fromkeys(general_urls))
        
        self.logger.send_log(
            message=f"Found {len(unique_urls)} general URLs",
            labels={"job": "web_crawler", "event": "general_urls_extracted"},
            metadata={"count": str(len(unique_urls))}
        )
        
        return unique_urls
    
    async def get_noticias_urls(self, num=100):
        NUM_PER_PAGE = 3
        noticias_urls = []
        
        self.logger.send_log(
            message=f"Starting extraction of noticias URLs (target: {num})",
            labels={"job": "web_crawler", "event": "extract_noticias_urls"},
            metadata={"target_count": str(num), "per_page": str(NUM_PER_PAGE)}
        )
        
        for start in range(0, num, NUM_PER_PAGE):
            noticias_page = f"{self.start_url}?start={start}#news"
            
            await self.page.goto(noticias_page)
            await self.page.wait_for_load_state('networkidle')
            
            news_links = await self.page.query_selector_all('.news-text a[href]')
            base_domain = urlparse(self.start_url).netloc
            
            for link in news_links:
                url = await self.parse_url(link, base_domain)
                if url and url not in self.urls_visited:
                    noticias_urls.append(url)
        
        self.logger.send_log(
            message=f"Finished extracting noticias URLs, total found: {len(noticias_urls)}",
            labels={"job": "web_crawler", "event": "noticias_extraction_complete"},
            metadata={"total_urls": str(len(noticias_urls))}
        )
        
        return noticias_urls
    
    async def extract_page_content(self, page):
        self.logger.send_log(
            message=f"Starting content extraction for page: {page}",
            labels={"job": "web_crawler", "event": "page_content_extraction"},
            metadata={"url": page}
        )
        
        await self.page.goto(page)
        title = await self.page.query_selector('h1')
        content = await self.page.query_selector('#main-box')
        
        title_text = await title.inner_text() if title else "No title"
        content_text = await content.inner_text() if content else "No content"
        
        page_data = {
            "title": title_text,
            "content": content_text
        }
        
        os.makedirs("data", exist_ok=True)
        filename = os.path.join("data", f"{page.replace('/', '_').replace(':', '')}.json")
        
        try:
            with open(filename, "w", encoding="utf-8") as f:
                json.dump(page_data, f, ensure_ascii=False, indent=2)
        except Exception as e:
            self.logger.send_log(
                message=f"Error saving content for page: {str(e)}",
                labels={"job": "web_crawler", "event": "page_content_save_error", "level": "error"},
                metadata={"url": page, "error": str(e)}
            )
            
    async def crawl(self):
        """
        Main crawling method that demonstrates usage of get_general_urls
        """
        print("Starting web crawling...")
        self.logger.send_log(
            message="Web crawling session started",
            labels={"job": "web_crawler", "event": "crawl_start"}
        )

        try:
            await self.init_browser()
            
            # Get general URLs from navbar
            general_urls = await self.get_general_urls()
            self.urls_pool.update(general_urls)
            print(f"Found {len(general_urls)} general URLs from navbar:")

            noticias_urls = await self.get_noticias_urls()
            self.urls_pool.update(noticias_urls)
            print(f"Found {len(noticias_urls)} noticias URLs:")

            total_urls = len(self.urls_pool)
            self.logger.send_log(
                message=f"Starting page crawling process with {total_urls} URLs in pool",
                labels={"job": "web_crawler", "event": "crawl_loop_start"},
                metadata={
                    "total_urls": str(total_urls),
                    "general_urls": str(len(general_urls)),
                    "noticias_urls": str(len(noticias_urls))
                }
            )

            processed_count = 0
            while self.urls_pool:
                current_url = self.urls_pool.pop()
                if current_url in self.urls_visited:
                    continue
                
                processed_count += 1
                print(f"Crawling: {current_url}")
                
                self.logger.send_log(
                    message=f"Processing URL ({processed_count}/{total_urls}): {current_url}",
                    labels={"job": "web_crawler", "event": "url_processing"},
                    metadata={
                        "url": current_url,
                        "processed_count": str(processed_count),
                        "total_count": str(total_urls)
                    }
                )
                
                try:
                    await self.extract_page_content(current_url)
                    self.urls_visited.add(current_url)
                    
                    # Extract new links from the current page
                    page_links = await self.page.query_selector_all('.substudies a[href]')
                    base_domain = urlparse(self.start_url).netloc
                    
                    for link in page_links:
                        url = await self.parse_url(link, base_domain)
                        if url and url not in self.urls_visited:
                            self.urls_pool.add(url)
                    
                    
                except Exception as e:
                    print(f"Error crawling {current_url}: {e}")
                    self.logger.send_log(
                        message=f"Error crawling URL: {str(e)}",
                        labels={"job": "web_crawler", "event": "url_crawl_error", "level": "error"},
                        metadata={"url": current_url, "error": str(e)}
                    )
            
            self.logger.send_log(
                message=f"Web crawling session completed successfully. Processed {len(self.urls_visited)} pages",
                labels={"job": "web_crawler", "event": "crawl_complete"},
                metadata={
                    "pages_processed": str(len(self.urls_visited)),
                    "total_urls_found": str(len(self.urls_visited) + len(self.urls_pool))
                }
            )
                
        except Exception as e:
            self.logger.send_log(
                message=f"Critical error during crawling session: {str(e)}",
                labels={"job": "web_crawler", "event": "crawl_critical_error", "level": "error"},
                metadata={"error": str(e)}
            )
            raise
        finally:
            await self.close()

# Example usage
async def main():
    crawler = WebCrawler(BASE_URL)
    try:
        results = await crawler.crawl()
        crawler.logger.send_log(
            message="Main function completed successfully",
            labels={"job": "web_crawler", "event": "main_complete"}
        )
        return results
    except Exception as e:
        print(f"Error during crawling: {e}")
        crawler.logger.send_log(
            message=f"Main function failed: {str(e)}",
            labels={"job": "web_crawler", "event": "main_error", "level": "error"},
            metadata={"error": str(e)}
        )
        await crawler.close()
        raise

if __name__ == "__main__":
    asyncio.run(main())
    
import requests
import json
from datetime import datetime
import pytz
from typing import Dict, List, Optional, Union
import logging
from collections import defaultdict

try:
    from loki_config import LOKI_URL, USER_ID, API_KEY
except ImportError:
    logging.warning("loki_config.py not found. Please create it from loki_config.example.py")
    LOKI_URL = None
    USER_ID = None
    API_KEY = None

class LokiLogger:
    def __init__(
        self, 
        loki_url: str = LOKI_URL,
        user_id: str = USER_ID,
        api_key: str = API_KEY,
        timezone: str = 'Europe/Madrid'
    ):
        """
        Initialize Loki logger
        
        Args:
            loki_url: URL of Loki server (default from config)
            user_id: User ID for authentication (default from config)
            api_key: API key for authentication (default from config)
            timezone: Timezone for timestamps
        """
        self.loki_url = f"{loki_url.rstrip('/')}/loki/api/v1/push"
        self.user_id = user_id
        self.api_key = api_key
        self.timezone = pytz.timezone(timezone)
        
        self.headers = {
            'Content-Type': 'application/json'
        }
        
        # Check if logger is properly configured
        self.isConfigured = bool(loki_url and user_id and api_key)
        if not self.isConfigured:
            logging.debug("LokiLogger is not properly configured. Missing loki_url, user_id, or api_key.")
        
    def _get_timestamp_ns(self) -> str:
        """Get current timestamp in nanoseconds as string"""
        return str(int(datetime.now(self.timezone).timestamp() * 1e9))
    
    def _format_stream_entry(
        self,
        message: str,
        timestamp: Optional[str] = None,
    ) -> List[str]:
        """Format a single stream entry"""
        entry = [timestamp or self._get_timestamp_ns(), message]
        return entry

    def send_log(
        self, 
        message: str,
        labels: Dict[str, str],
        timestamp: Optional[str] = None
    ) -> requests.Response:
        """
        Send single log message to Loki
        
        Args:
            message: Log message
            labels: Dictionary of labels
            timestamp: Optional timestamp in nanoseconds
            
        Returns:
            Response from Loki API
        """
        if not self.isConfigured:
            logging.info("LokiLogger is not configured. Skipping log send.")
            logging.debug(f"Log message: {message}, labels: {labels}, timestamp: {timestamp}")
            return None
        payload = {
            "streams": [
                {
                    "stream": {
                        "language": "Python",
                        "source": "Code",
                        "level": "info",
                        **labels
                    },
                    "values": [
                        self._format_stream_entry(message, timestamp)
                    ]
                }
            ]
        }

        return self._send_payload(payload)

    def _send_payload(self, payload: Dict) -> requests.Response:
        """Send payload to Loki"""
        try:
            response = requests.post(
                self.loki_url,
                auth=(self.user_id, self.api_key),
                json=payload,
                headers=self.headers,
                timeout=5
            )
            response.raise_for_status()
            return response
            
        except requests.exceptions.RequestException as e:
            logging.error(f"Failed to send logs to Loki: {str(e)}")
            raise

# Example usage
if __name__ == "__main__":
    # Initialize logger (will use config file defaults)
    logger = LokiLogger()
    logger.send_log(
        message="This is a test log message from LokiLogger",
        labels={
            "job": "test_logger",
            "environment": "debug",
            "level": "info"
        }
    )
        

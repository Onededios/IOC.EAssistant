import requests
import json
from datetime import datetime, time
import pytz
from typing import Dict, List, Optional, Union
import logging
from collections import defaultdict
from loki_config import LOKI_URL, USER_ID, API_KEY

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
    
    try:
        # Example 1: Simple batch of logs with different labels
        entries = [
            {
                "message": "User login successful",
                "labels": {
                    "job": "auth_service",
                    "environment": "production",
                    "level": "info"
                }
            },
            {
                "message": "Database query completed",
                "labels": {
                    "job": "db_service",
                    "environment": "production",
                    "level": "info"
                }
            }
        ]
        
        logger.send_batch(entries)
        
        # Example 2: Batch with metadata and custom timestamps
        entries_with_metadata = [
            {
                "message": "API request failed",
                "labels": {
                    "job": "api_service",
                    "level": "error"
                },
                "metadata": {
                    "trace_id": "0242ac120002",
                    "user_id": "12345"
                },
                "timestamp": "1645123456000000000"  # Optional custom timestamp
            },
            {
                "message": "Cache miss",
                "labels": {
                    "job": "api_service",
                    "level": "warn"
                },
                "metadata": {
                    "cache_key": "user:12345"
                }
            }
        ]
        
        # sending with groiups via labels (entries with equilent labels will be in one stream)
        logger.send_batch(entries_with_metadata, group_by_labels=True)
        
    except Exception as e:
        print(f"Error: {e}")

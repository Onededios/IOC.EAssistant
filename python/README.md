# Python Module - IOC.EAssistant

This module contains the Python components for the IOC.EAssistant project, including a web crawler, Loki logger integration, and web API.

## Installation

### Prerequisites
- Python 3.7 or higher
- pip package manager

### Setup with Virtual Environment (Recommended for Local Development)

It's recommended to use a virtual environment to isolate project dependencies:

```bash
# Navigate to the python folder
cd python

# Create a virtual environment
python -m venv venv

# Activate the virtual environment
# On Windows (PowerShell):
.\venv\Scripts\Activate.ps1

# On Windows (Command Prompt):
.\venv\Scripts\activate.bat

# On Linux/Mac:
source venv/bin/activate

# Install required packages
pip install -r requirements.txt
```

### Install Without Virtual Environment

If you prefer to install globally:

```bash
cd python
pip install -r requirements.txt
```

## Configuration

### Loki Logger Configuration

Before using the Loki logger, you need to configure your Loki server credentials:

1. Copy the example configuration file:
   ```bash
   cp loki_config.example.py loki_config.py
   ```

2. Edit `loki_config.py` with your Loki server details:
   ```python
   # Loki server URL
   LOKI_URL = "http://your-loki-server:3100"
   
   # Authentication credentials
   USER_ID = "your_user_id"
   API_KEY = "your_api_key"
   ```

3. Save the file. The `loki_logger.py` will automatically use these settings.

**Note**: Make sure `loki_config.py` is added to `.gitignore` to avoid committing sensitive credentials.

## Usage

### Crawler

The crawler module is used to scrape and process data from IOC websites:

```bash
# Run the crawler
python crawler.py
```

The crawler will:
- Fetch latest news and updates from IOC education portal
- Save the data in JSON format to the `data/` directory
- Log activities using the Loki logger (if configured)

**Data Storage**: Crawled data is stored in the `data/` folder with filenames based on the source URL.

### Loki Logger

The Loki logger can be used independently in your Python scripts:

```python
from loki_logger import LokiLogger

# Initialize logger (uses configuration from loki_config.py)
logger = LokiLogger()

# Send a log message
logger.send_log(
    message="Your log message here",
    labels={
        "job": "my_job",
        "environment": "production",
        "level": "info"
    }
)

# Check if logger is properly configured
if logger.isConfigured:
    print("Logger is ready to use")
else:
    print("Logger needs configuration")
```

### Web API

The web API provides endpoints for accessing the crawler data and other functionality.

```bash
# Run the web API server
python web.py
```

**Note**: Detailed API documentation and usage examples will be added in future updates.

## Project Structure

```
python/
├── crawler.py              # Web crawler for IOC education portal
├── loki_logger.py          # Loki logging integration
├── loki_config.py.example  # Example configuration file
├── web.py                  # Web API server
├── requirements.txt        # Python dependencies
├── data/                   # Crawled data storage (JSON files)
└── README.md              # This file
```

## Troubleshooting

### Loki Logger Not Working
- Verify that `loki_config.py` exists and contains valid credentials
- Check that your Loki server is accessible
- Ensure the `isConfigured` property is `True` after initialization

### Import Errors
- Make sure all dependencies are installed: `pip install -r requirements.txt`
- Verify you're using Python 3.7 or higher: `python --version`
- If using a virtual environment, ensure it's activated

### Data Directory Issues
- The `data/` directory should be created automatically by the crawler
- Ensure you have write permissions in the python folder

## Future Development

- [ ] Complete Web API documentation
- [ ] Add API endpoint examples
- [ ] Implement authentication for API
- [ ] Add automated testing
- [ ] Improve error handling and logging

## Contributing

Please refer to the main project's CONTRIBUTING.md file for contribution guidelines.

## License

See the LICENSE file in the root directory of the project.

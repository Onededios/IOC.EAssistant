# IOC.EAssistant - Administrator's Guide

---

## Table of Contents

- [System Overview](#system-overview)
- [Prerequisites](#prerequisites)
- [Architecture Components](#architecture-components)
- [Installation](#installation)
  - [.NET Gateway API Installation](#net-gateway-api-installation)
- [Configuration](#configuration)
  - [Database Configuration](#database-configuration)
  - [Loki Logging Configuration](#loki-logging-configuration)
  - [API Configuration](#api-configuration)
- [Deployment](#deployment)
  - [Development Environment](#development-environment)
  - [Production Environment](#production-environment)
- [Verification & Testing](#verification--testing)
- [Monitoring & Maintenance](#monitoring--maintenance)
- [Additional Resources](#additional-resources)
- [Support & Contact](#support--contact)

---

## System Overview

IOC.EAssistant is a multi-component AI-powered educational assistant platform consisting of:

- **.NET 9 Gateway API**: Main API gateway handling requests, routing, and business logic
- **Python Web Crawler**: Data collection service for IOC educational content
- **Python Web API**: Chatbot and AI integration services
- **External Services**:
  - NeonDB as PostgreSQL database
  - Grafana Loki for centralized logging

---

## Prerequisites

### Required Software

#### For .NET Gateway API

- **.NET 9 SDK** or later

  - Download: [https://dotnet.microsoft.com/download/dotnet/9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
  - Verify installation: `dotnet --version`

- **Visual Studio 2022** (17.8+) or **Visual Studio Code** with C# extension

### External Services (Remote)

The following services **must be configured remotely** and accessible from your deployment environment:

#### Database Server

- **NeonDB as PostgreSQL database**
- Network accessible from your deployment environment
- Database and user credentials ready

#### Grafana Loki (Logging)

- **Loki 2.0+** instance
- HTTP/HTTPS endpoint accessible
- Authentication credentials (username/password or API key)

### Network Requirements

- **Outbound HTTPS (443)**: For accessing external services (Loki, database if cloud-hosted)
- **Inbound Port 5001**: For .NET Gateway API (configurable)
- **Database Port**: Typically 5432 (PostgreSQL) or 1433 (SQL Server)
- **Loki Port**: Typically 3100 or 443 (if using HTTPS)

---

## Architecture Components

### .NET Gateway API (Port 5001)

The main API gateway built on .NET 9 with the following structure:

```
IOC.EAssistant.Gateway/
 ├ IOC.EAssistant.Gateway.Api/                            # Main API project
 ├ IOC.EAssistant.Gateway.Library.Contracts/              # Business layer contracts
 ├ IOC.EAssistant.Gateway.Library.Implementation/         # Business layer implementation
 ├ IOC.EAssistant.Gateway.Library.Entities/               # Domain entities
 ├ IOC.EAssistant.Gateway.Infrastructure.Contracts/       # Infrastructure layer contracts
 ├ IOC.EAssistant.Gateway.Infrastructure.Implementation/  # Infrastructure layer (DB access, logging)
 ├ IOC.EAssistant.Gateway.XCutting.Logging/               # Logging infrastructure
 └ IOC.EAssistant.Gateway.XCutting.Result/                # Result pattern implementation
```

**Key Features:**

- RESTful API endpoints
- Swagger/OpenAPI documentation
- Database access layer (Dapper ORM)
- Integrated Loki logging
- CORS support

---

## Installation

### .NET Gateway API Installation

#### Step 1: Clone the Repository

```bash
# Clone the repository
git clone https://github.com/Onededios/IOC.EAssistant.git
cd IOC.EAssistant
```

#### Step 2: Navigate to Gateway Directory

```bash
cd IOC.EAssistant.Gateway
```

#### Step 3: Restore NuGet Packages

```bash
dotnet restore
```

This will install all required packages including:

- Microsoft.AspNetCore.OpenApi (9.0.9)
- Serilog and Serilog.AspNetCore (for logging)
- Serilog.Sinks.Grafana.Loki (8.3.1)
- Swashbuckle.AspNetCore (9.0.6) - for API documentation
- Dapper (for database access)

#### Step 4: Build the Solution

```bash
dotnet build
```

Verify that the build completes successfully without errors.

---

---

## Configuration

### Model Configuration

1. Run the Python Web API and Web Crawler services as per their respective admin guides to ensure models are downloaded and configured.

2. Note the URLs where these services are running (e.g., `http://localhost:8000` for Web API).

3. Update the .NET Gateway API configuration to point to these services (see API Configuration section).

#### By Using User Secrets

```bash
cd IOC.EAssistant.Gateway/IOC.EAssistant.Gateway.Api
dotnet user-secrets set "EASSISTANT_URI" "http://localhost:8000"
```

#### As env file

```json
{
	"EASSISTANT_URI": "http://localhost:8000"
}
```

### Database Configuration

#### Step 1: Prepare Your Database

1. Create a database on your remote PostgreSQL or SQL Server instance
2. Run any required schema migrations/scripts (if provided)
3. Note the connection string

**Example PostgreSQL Connection String:**

```
Host=your-db-server.example.com;Port=5432;Database=eassistant;Username=eassistant_user;Password=your_secure_password;SSL Mode=Require;
```

**Example SQL Server Connection String:**

```
Server=your-db-server.example.com,1433;Database=eassistant;User Id=eassistant_user;Password=your_secure_password;Encrypt=True;TrustServerCertificate=False;
```

#### Step 2: Configure Connection String

**By User Secrets (Recommended for Development)**

```bash
cd IOC.EAssistant.Gateway/IOC.EAssistant.Gateway.Api
dotnet user-secrets init
dotnet user-secrets set "EASSISTANT_CONNSTR" "your_connection_string_here"
```

**As env file:**

Edit `IOC.EAssistant.Gateway.Api/appsettings.json`:

```json
{
	"EASSISTANT_CONNSTR": "your_connection_string_here"
}
```

> **Security Warning**: Never commit connection strings to source control!

#### Step 3: Create database tables

Run the following command to create necessary tables:

```sql
--- Sessions table

CREATE TABLE public.sessions (
    id UUID PRIMARY KEY,
    created_at DATE NOT NULL,
    CONSTRAINT sessions_pkey PRIMARY KEY (id)
);

CREATE UNIQUE INDEX sessions_pkey ON public.sessions USING BTREE (id);

--- Conversations table

CREATE TABLE public.conversations (
    id UUID PRIMARY KEY,
    session_id UUID NOT NULL,
    title VARCHAR(30),
    created_at TIMESTAMP NOT NULL,
    CONSTRAINT conversation_session
      FOREIGN KEY (session_id)
      REFERENCES public.sessions(id)
      ON DELETE CASCADE,
    CONSTRAINT conversations_pkey PRIMARY KEY (id)
);

CREATE UNIQUE INDEX conversations_pkey ON public.conversations USING BTREE (id);

--- Questions table

CREATE TABLE public.questions (
    id UUID PRIMARY KEY,
    question TEXT NOT NULL,
    created_at DATE NOT NULL,
    token_count INTEGER NOT NULL,
    metadata JSONB,
    conversation_id UUID NOT NULL,
    index INTEGER NOT NULL,
    CONSTRAINT question_conversation
      FOREIGN KEY (conversation_id)
      REFERENCES public.conversations(id)
      ON DELETE CASCADE,
    CONSTRAINT questions_pk PRIMARY KEY (id)
);

CREATE UNIQUE INDEX questions_pk ON public.questions USING BTREE (id);

--- Answers table

CREATE TABLE public.answers (
    id UUID PRIMARY KEY,
    question_id UUID UNIQUE NOT NULL,
    answer TEXT NOT NULL,
    created_at DATE NOT NULL,
    token_count INTEGER NOT NULL,
    metadata JSONB,
    sources JSONB,
    CONSTRAINT answer_question
      FOREIGN KEY (question_id)
      REFERENCES public.questions(id)
      ON DELETE CASCADE,
    CONSTRAINT answers_question_id_key UNIQUE (question_id),
    CONSTRAINT answers_pk1 PRIMARY KEY (id)
);

CREATE UNIQUE INDEX answers_pk1 ON public.answers USING BTREE (id);
CREATE UNIQUE INDEX answers_question_id_key ON public.answers USING BTREE (question_id);
```

---

### Loki Logging Configuration

#### For .NET Gateway API

Configure the following environment variables:

**By Using User Secrets:**

```bash
cd IOC.EAssistant.Gateway/IOC.EAssistant.Gateway.Api
dotnet user-secrets set "LOKI_URL" "https://your-loki-instance.example.com/loki/api/v1/push"
dotnet user-secrets set "LOKI_USERNAME" "your_loki_username"
dotnet user-secrets set "LOKI_PASSWORD" "your_loki_password"
```

**As env file:**

```json
{
	"LOKI_URL": "https://your-loki-instance.example.com/loki/api/v1/push",
	"LOKI_USERNAME": "your_loki_username",
	"LOKI_PASSWORD": "your_loki_password"
}
```

> **Important**: Add `.env` to your `.gitignore` to prevent committing credentials!

---

### API Configuration

#### .NET Gateway API Settings

Edit `IOC.EAssistant.Gateway.Api/appsettings.json` for general settings:

```json
{
	"Logging": {
		"LogLevel": {
			"Default": "Information",
			"Microsoft": "Information",
			"Microsoft.AspNetCore": "Warning"
		}
	},
	"CORS": {
		"__DefaultCorsPolicy": {
			"Origins": ["https://your-frontend-domain.com"],
			"Methods": ["GET", "POST", "PUT", "DELETE"],
			"Headers": ["Content-Type", "Authorization"],
			"ExposedHeaders": []
		}
	},
	"AllowedHosts": "*",
	"EASSISTANT_URI": "http://localhost:8000"
}
```

**Configuration Options:**

- **CORS Origins**: Update to match your frontend domain(s)
  - Use `["*"]` for development only
  - Specify exact domains for production
- **EASSISTANT_URI**: URL of the Python Web API (if using)
- **AllowedHosts**: Restrict to specific hostnames in production

#### Configuring HTTPS Port

Edit `IOC.EAssistant.Gateway.Api/Properties/launchSettings.json`:

```json
{
	"profiles": {
		"https": {
			"commandName": "Project",
			"launchUrl": "swagger",
			"environmentVariables": {
				"ASPNETCORE_ENVIRONMENT": "Production"
			},
			"applicationUrl": "https://0.0.0.0:5001"
		}
	}
}
```

---

## Deployment

### Development Environment

#### Running the .NET Gateway API

```bash
cd IOC.EAssistant.Gateway/IOC.EAssistant.Gateway.Api
dotnet run
```

The API will be available at:

- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

### Production Environment

#### Reverse Proxy

Configure a reverse proxy (e.g., Nginx, Apache) to forward requests to the .NET Gateway API.

---

## Verification & Testing

### Test .NET Gateway API

**1. Check if the service is running:**

```bash
curl -k https://localhost:5001/swagger/index.html
```

**2. Test API endpoint:**

```bash
curl -k -X GET https://localhost:5001/api/health
```

**3. View Swagger Documentation:**
Open browser: `https://localhost:5001/swagger`

### Test Logging Integration

**1. Check .NET logs in console output**

Open the terminal where the .NET Gateway API is running and look for log entries.

**2. Check Loki:**

- Access your Grafana instance
- Query for logs with label `app="eassistant-gateway"`
- Verify logs are being received

---

## Monitoring & Maintenance

### Log Monitoring

**Using Grafana:**

1. Access your Grafana instance
2. Navigate to Explore
3. Select Loki data source
4. Query examples:

```logql
{app="eassistant-gateway"} |= "error"
{job="web_crawler"}
{environment="production", level="error"}
```

---

## Additional Resources

- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Serilog Documentation](https://serilog.net/)
- [Grafana Loki Documentation](https://grafana.com/docs/loki/latest/)
- [Flask Documentation](https://flask.palletsprojects.com/)
- [Playwright Documentation](https://playwright.dev/python/)

---

## Support & Contact

For technical support or questions:

- **GitHub Issues**: [https://github.com/Onededios/IOC.EAssistant/issues](https://github.com/Onededios/IOC.EAssistant/issues)
- **Documentation**: [Project Wiki](https://github.com/Onededios/IOC.EAssistant/wiki)

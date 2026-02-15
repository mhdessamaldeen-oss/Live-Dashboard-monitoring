# Deployment Guide

## Docker Deployment (Recommended)
The system is fully containerized and can be deployed with Docker Compose.

### Prerequisites
- Docker Desktop / Engine
- Minimum 4GB RAM

### Commands
1. **Startup**: `docker-compose up --build -d`
2. **Shutdown**: `docker-compose down`
3. **Full Reset (Wipe Data)**: `docker-compose down -v`

### Service Access
- **Frontend**: http://localhost:4200
- **API**: http://localhost:5000/swagger
- **Hangfire Dashboard**: http://localhost:5000/hangfire

## Manual Publish

### Backend (.NET)
1. `cd app`
2. `dotnet publish src/WebAPI -c Release -o ./publish`
3. Configure connection strings in `appsettings.Production.json` or Environment Variables.
4. Run: `dotnet WebAPI.dll`

### Frontend (Angular)
1. `cd frontend`
2. `npm install`
3. `ng build --configuration production`
4. Serve the `dist/` folder using Nginx, Apache, or IIS.

## Configuration Variables
| Name | Description | Default |
|------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` for deployment | `Development` |
| `SA_PASSWORD` | SQL Server SA password | `YourStrong!Passw0rd` |
| `JWT_SECRET` | Secret key for JWT signing | `SuperSecretKey...` |
| `DOCKER_USERNAME` | Docker Hub username (for prod compose) | `essammas` |

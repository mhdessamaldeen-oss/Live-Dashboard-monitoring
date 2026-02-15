# Real-Time Monitoring Dashboard

A system monitoring utility built with ASP.NET Core 10 and Angular 20, utilizing Clean Architecture.

## Documentation Links
- [Architecture Details](./ARCHITECTURE.md) - Deep dive into layers and patterns.
- [API Specification](./API.md) - REST endpoints and SignalR events.
- [Deployment Guide](./DEPLOYMENT.md) - Docker and manual setup.
- [AI Usage Disclosure](./AI_USAGE.md) - Details on AI-assisted development.

## Quick Start
Run the following to start the full stack (API, Frontend, SQL Server, Redis):

```bash
docker-compose up --build -d
```

### Access Points
- **Dashboard**: http://localhost:4200
- **Swagger UI**: http://localhost:5000/swagger
- **Hangfire**: http://localhost:5000/hangfire

## Production
To use production-ready images:
```bash
docker-compose -f docker-compose.prod.yml up -d
```

## Features
- SignalR-based real-time metric updates.
- Background metric collection via Hangfire.
- Automatic threshold evaluation and alerting.
- PDF/CSV report generation engine.
- Role-based security with JWT (Admin/User).

## Login Credentials
- **Admin**: `admin@demo.com` / `Admin123!`
- **User**: `user@demo.com` / `User123!`

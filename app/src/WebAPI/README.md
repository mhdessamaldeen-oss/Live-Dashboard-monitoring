# Web API Layer

## Overview
The WebAPI layer is the entry point of the application. It provides a RESTful API and real-time hubs for frontend consumers.

## Features
- **Controllers**: Thin controllers that delegate work to MediatR.
- **Hubs**: Real-time server-to-client notifications via SignalR.
- **Security**: JWT Bearer Authentication and Role-based Authorization.
- **Swagger**: API documentation and interactive testing UI.
- **Middleware**: Global exception handling and performance monitoring.

## Folder Structure
- **Controllers**: Resource-based API endpoints.
- **Hubs**: SignalR hub definitions.
- **Middleware**: Global request pipeline extensions.
- **Services**: Web-specific services (e.g., `CurrentUserService`).

# Infrastructure Layer

## Overview
The Infrastructure layer provides implementations for the interfaces defined in the Application layer. It handles external concerns like databases, identity, and third-party APIs.

## Technologies
- **EF Core**: Database persistence with SQL Server.
- **Hangfire**: Background job processing and scheduling.
- **Redis**: Distributed caching and SignalR backplane.
- **SignalR**: Real-time communication.

## Folder Structure
- **Persistence**: DbContext, Migrations, and SQL Configurations.
- **Repositories**: Concrete data access implementations.
- **Services**: Implementations for Auth, Caching, and Metrics collection.
- **External**: Integrations with outside systems.

## Key Responsibilities
- Implementing Repository interfaces.
- Configuring the database schema using Fluent API.
- Managing background job lifecycles.

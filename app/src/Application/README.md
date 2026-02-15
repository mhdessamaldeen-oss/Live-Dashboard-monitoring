# Application Layer

## Overview
The Application layer contains the application logic and use cases. It coordinates the data flow between the Domain entities and external consumers.

## Patterns & Practices
- **CQRS**: Command Query Responsibility Segregation using **MediatR**.
- **Handlers**: Every business action is a Command or Query handled by a specific class.
- **Validation**: Input validation using **FluentValidation**.
- **Mapping**: Entity to DTO conversion using **AutoMapper**.
- **Interfaces**: Defines abstractions for Infrastructure services (e.g., `ICacheService`).

## Folder Structure
- **Common**: Shared behaviors (Logging, Validation, Performance).
- **DTOs**: Data Transfer Objects for cross-layer communication.
- **Features**: Grouped by resource (Servers, Metrics, Users). Each folder contains Commands, Queries, and Handlers.
- **Interfaces**: Abstractions implemented by the Infrastructure layer.

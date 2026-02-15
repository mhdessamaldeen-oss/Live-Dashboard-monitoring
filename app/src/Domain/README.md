# Domain Layer

## Overview
The Domain layer contains the enterprise logic and types. It is the core of the application and should have no dependencies on other layers or external libraries (like EF Core).

## Folder Structure
- **Entities**: Core business objects that reflect the system's state.
- **Enums**: Enumerations defining status codes, severities, etc.
- **Common**: Base classes and shared domain logic (e.g., `BaseEntity`).
- **Exceptions**: Custom domain-specific exceptions.
- **ValueObjects**: Complex types that don't have an identity.

## Key Principles
- **Persistence Ignorance**: Entities are not aware of how they are saved.
- **Business Logic**: Methods on entities should encapsulate business rules.

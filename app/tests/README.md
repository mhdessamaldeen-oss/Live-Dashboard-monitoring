# Test Suite

## Overview
This directory contains the automated test suite for the backend application, covering Domain, Application, and WebAPI layers.

## Structure
- **Domain.Tests**: Pure unit tests for entities and domain logic (e.g., threshold evaluation).
- **Application.Tests**: Unit tests for MediatR handers using Moq for dependency isolation.
- **WebAPI.IntegrationTests**: End-to-end API tests using `WebApplicationFactory` and a test database.

## Running Tests
Run all tests from the repository root or `app` directory:
```bash
dotnet test
```

## Tools Used
- **xUnit**: Testing framework.
- **Moq**: Mocking library for dependencies.
- **FluentAssertions**: For readable, expressive assertions.

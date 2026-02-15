# ───────────────────────────────────────────────
# Stage 1: Build
# ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/WebAPI/WebAPI.csproj src/WebAPI/
RUN dotnet restore src/WebAPI/WebAPI.csproj

# Copy everything else
COPY . .

# Build and publish
WORKDIR /src/src/WebAPI
RUN dotnet publish -c Release -o /app/publish --no-restore

# ───────────────────────────────────────────────
# Stage 2: Runtime
# ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for health check
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create directory for reports
RUN mkdir -p /app/App_Data/reports

# Copy published output
COPY --from=build /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "WebAPI.dll"]

# Live Monitoring Dashboard - Frontend

## Overview
A modern, responsive monitoring dashboard built with **Angular 17+**, **NgRx**, and **Angular Material**.

## Features
- **Real-Time Dashboards**: Live charts using ApexCharts.
- **State Management**: Scalable state handling with NgRx.
- **Reactive Extensions**: Heavy use of RxJS for event streams.
- **Theming**: Custom material theme with dark mode support.

## Project Structure
- `src/app/core`: Singleton services (Auth, SignalR, Interceptors).
- `src/app/features`: Feature-based modules (Servers, Alerts, Reports).
- `src/app/shared`: Shared UI components, pipes, and directives.
- `src/app/store`: NgRx global state definitions.

## Getting Started
1. **Install Dependencies**:
   ```bash
   npm install
   ```
2. **Run Locally**:
   ```bash
   npm start
   ```
3. **Run Unit Tests**:
   ```bash
   npm run test
   ```

## Development Tools
- **Angular CLI**: version 17+
- **Karma/Jasmine**: Unit testing.
- **ApexCharts**: Data visualization.

## Environment Configuration
Update the `src/environments/environment.ts` file to match your backend API settings:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1',
  signalrUrl: 'http://localhost:5000/hubs/monitoring'
};
```

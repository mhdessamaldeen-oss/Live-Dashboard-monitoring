# API Documentation — Live Monitoring Dashboard

This document outlines the REST API and SignalR Hub events for the Live Monitoring Dashboard.

## Base URL
- **Local Dev**: `https://localhost:5001/api/v1`
- **Docker/Production**: `http://localhost:8080/api/v1` (via Nginx)

---

## Authentication
The API uses **JWT (JSON Web Token)** for authorization. 

### Flow:
1. Client sends `POST /auth/login` with Email/Password.
2. Server validates credentials against BCrypt hash.
3. Server returns an `AccessToken` and a `RefreshToken`.
4. Client includes AccessToken in the `Authorization: Bearer <token>` header for all subsequent requests.
5. When AccessToken expires, client uses `POST /auth/refresh` to get a new pair.

### Endpoints
| Endpoint | Method | Description | Roles |
|----------|--------|-------------|-------|
| `/auth/login` | `POST` | Get JWT token (email/password) | All |
| `/auth/register`| `POST` | Register new user | All |
| `/auth/refresh` | `POST` | Exchange refresh token for new access token | All |

**Login Request Example:**
```json
{
  "email": "admin@demo.com",
  "password": "Password123!"
}
```

---

## Servers
Manage and monitor infrastructure nodes.

| Endpoint | Method | Description | Roles |
|----------|--------|-------------|-------|
| `/servers` | `GET` | List servers (paged/filtered) | User, Admin |
| `/servers/{id}`| `GET` | Get server details | User, Admin |
| `/servers` | `POST` | Provision a new server | Admin |
| `/servers/{id}`| `PUT` | Update server configuration | Admin |
| `/servers/{id}`| `DELETE`| Remove a server | Admin |

---

## Metrics
Access performance data for specific servers.

| Endpoint | Method | Description | Roles |
|----------|--------|-------------|-------|
| `/servers/{id}/metrics` | `GET` | Get historical metrics (paged) | User, Admin |
| `/servers/{id}/metrics/latest` | `GET` | Get latest metric | User, Admin |

---

## Alerts
System threshold notifications.

| Endpoint | Method | Description | Roles |
|----------|--------|-------------|-------|
| `/alerts` | `GET` | List alerts (paged/filtered) | User, Admin |
| `/alerts/{id}/resolve` | `POST` | Mark an alert as resolved | User, Admin |
| `/alerts/summary` | `GET` | Get total counts (Active/Resolved) | User, Admin |

---

## Reports
Generate and download PDF/CSV system reports.

| Endpoint | Method | Description | Roles |
|----------|--------|-------------|-------|
| `/reports` | `POST` | Trigger report generation (Async) | User, Admin |
| `/reports` | `GET` | List generated reports | User, Admin |
| `/reports/{id}/download`| `GET` | Download report file | User, Admin |

---

## SignalR (Real-Time Hub)
**Hub URL**: `/hubs/monitoring`

### Client Groups
- `dashboard`: Join this group to receive ALL server updates.
- `server-{serverId}`: Receive metrics for a specific server.

### Events (Server -> Client)
| Event | Payload | Description |
|-------|---------|-------------|
| `ReceiveMetricUpdate` | `MetricDto` | Performance data updates. |
| `ReceiveAlertTriggered` | `AlertDto` | Sent when a threshold is breached. |
| `ReceiveAlertResolved` | `number` (id) | Sent when an alert is resolved. |
| `ReceivePresenceChanged`| `number` (count)| Users online count. |
| `ReceiveReportReady` | `ReportDto` | Notification when a report is generated. |

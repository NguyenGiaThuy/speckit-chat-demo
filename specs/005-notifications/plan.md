# Implementation Plan: Notification Service

## Tech Stack
- Runtime: .NET 9 minimal API + System.Net.WebSockets
- Database: PostgreSQL (EF Core 9)
- Events: Redis Pub/Sub (StackExchange.Redis)
- Container: Docker (port 8084)

## Architecture

```
NotificationService/
├── Endpoints/
│   ├── NotificationEndpoints.cs
│   └── WebSocketEndpoint.cs
├── Services/
│   ├── NotificationService.cs
│   ├── EventListenerService.cs   (Redis subscriber, IHostedService)
│   └── WebSocketManager.cs
├── Data/
│   ├── AppDbContext.cs
│   └── Entities/Notification.cs
├── Dockerfile
└── Program.cs
```

## Environment Variables
```
DB_CONNECTION_STRING=...
REDIS_CONNECTION=redis:6379
JWT_PUBLIC_KEY=...
```

## Docker Compose Entry
```yaml
notification-service:
  build: ./services/NotificationService
  ports:
    - "8084:8084"
  depends_on:
    - postgres
    - redis
```

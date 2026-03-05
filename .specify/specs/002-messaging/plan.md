# Implementation Plan: Real-Time Messaging Service

## Tech Stack
- Runtime: .NET 9 minimal API + System.Net.WebSockets
- Database: PostgreSQL (via EF Core 9)
- Real-time: Redis Pub/Sub (StackExchange.Redis) → WebSocket fan-out
- Container: Docker (port 8081)

## Architecture

```
MessagingService/
├── Endpoints/
│   ├── MessageEndpoints.cs    (REST CRUD)
│   └── WebSocketEndpoint.cs   (ws upgrade)
├── Services/
│   ├── MessageService.cs
│   ├── WebSocketManager.cs    (connection registry)
│   └── RedisPubSubService.cs  (publish/subscribe)
├── Data/
│   ├── AppDbContext.cs
│   └── Entities/Message.cs
├── DTOs/
│   ├── SendMessageRequest.cs
│   └── MessageDto.cs
├── Dockerfile
└── Program.cs
```

## Real-Time Flow
```
Client → POST /channels/{id}/messages
  → MessagingService saves to DB
  → Publishes event to Redis channel "channel:{id}"
  → RedisPubSubService receives event
  → WebSocketManager fans out to all subscribed connections
```

## Service Dependencies
- PostgreSQL (messaging_db)
- Redis (pub/sub)
- IdentityService (JWT public key for validation)

## Environment Variables
```
DB_CONNECTION_STRING=...
REDIS_CONNECTION=redis:6379
JWT_PUBLIC_KEY=<RS256 public key PEM>
JWT_ISSUER=speckit-chat
```

## Docker Compose Entry
```yaml
messaging-service:
  build: ./services/MessagingService
  ports:
    - "8081:8081"
  depends_on:
    - postgres
    - redis
```

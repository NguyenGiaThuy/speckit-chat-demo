# Implementation Plan: Presence Service

## Tech Stack
- Runtime: .NET 9 minimal API + System.Net.WebSockets
- State: Redis (StackExchange.Redis)
- Container: Docker (port 8083)

## Architecture

```
PresenceService/
├── Endpoints/
│   ├── PresenceEndpoints.cs
│   └── WebSocketEndpoint.cs
├── Services/
│   ├── PresenceService.cs      (Redis read/write)
│   ├── HeartbeatService.cs     (TTL refresh)
│   └── RedisPubSubService.cs
├── Dockerfile
└── Program.cs
```

## Environment Variables
```
REDIS_CONNECTION=redis:6379
JWT_PUBLIC_KEY=...
GUILD_SERVICE_URL=http://guild-service:8082
```

## Docker Compose Entry
```yaml
presence-service:
  build: ./services/PresenceService
  ports:
    - "8083:8083"
  depends_on:
    - redis
```

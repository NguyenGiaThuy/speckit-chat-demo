# Implementation Plan: Guild & Channel Service

## Tech Stack
- Runtime: .NET 9 minimal API
- Database: PostgreSQL (EF Core 9)
- Container: Docker (port 8082)

## Architecture

```
GuildService/
├── Endpoints/
│   ├── GuildEndpoints.cs
│   ├── ChannelEndpoints.cs
│   └── InviteEndpoints.cs
├── Services/
│   ├── GuildService.cs
│   ├── ChannelService.cs
│   └── InviteService.cs
├── Data/
│   ├── AppDbContext.cs
│   ├── Entities/Guild.cs
│   ├── Entities/Channel.cs
│   ├── Entities/Member.cs
│   └── Entities/GuildInvite.cs
├── Dockerfile
└── Program.cs
```

## Environment Variables
```
DB_CONNECTION_STRING=...
JWT_PUBLIC_KEY=...
APP_BASE_URL=http://localhost:8082
```

## Docker Compose Entry
```yaml
guild-service:
  build: ./services/GuildService
  ports:
    - "8082:8082"
  depends_on:
    - postgres
```

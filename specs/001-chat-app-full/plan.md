# Implementation Plan: Full Chat App (branch-01)

> ⚠️ This plan attempts to cover the entire application at once — 7 services, 
> 10+ data models, 40+ endpoints. Compare with feature branches where each plan
> covers exactly one service.

## Tech Stack
- .NET 9 microservices (Identity, Messaging, Guild, Presence, Notification, Voice, Admin)
- PostgreSQL (one DB per service)
- Redis (pub/sub, presence, cache)
- LiveKit (voice/video)
- Docker Compose

## Services to Build Simultaneously
1. IdentityService (auth, users, 2FA)
2. MessagingService (text, files, reactions, search, DMs)
3. GuildService (guilds, channels, roles, permissions, invites)
4. PresenceService (status, typing)
5. NotificationService (mentions, DMs, push)
6. VoiceService (LiveKit integration)
7. AdminService (audit log, moderation)

## Shared Infrastructure
- API Gateway (YARP reverse proxy)
- Shared JWT validation middleware
- Shared OpenAPI contracts package
- Shared Docker Compose network

## Architecture Diagram
```
Client
  └─► API Gateway (YARP :80)
        ├─► IdentityService   :8080
        ├─► MessagingService  :8081
        ├─► GuildService      :8082
        ├─► PresenceService   :8083
        ├─► NotificationService :8084
        ├─► VoiceService      :8085
        └─► AdminService      :8086

Infrastructure:
  postgres  (7 databases)
  redis     (pub/sub + cache)
  livekit   (voice/video)
```

## Known Challenges with This Approach
- All 7 services must be designed in parallel with circular dependencies
- Role/permission system spans Guild + all other services — no clear owner
- File upload design affects Messaging, Admin, and potentially Identity
- Testing requires all services running together from day one
- PR review for this branch would require reviewing all 7 services at once
- Any spec change affects multiple services simultaneously

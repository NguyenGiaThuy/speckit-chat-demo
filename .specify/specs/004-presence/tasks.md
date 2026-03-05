# Tasks: Presence Service

## Phase 1 — Scaffolding
- [ ] Create `services/PresenceService` .NET 9 minimal API
- [ ] Add StackExchange.Redis
- [ ] Dockerfile and docker-compose entry

## Phase 2 — Presence Endpoints
- [ ] GET /guilds/{id}/presence — fetch online members from Redis set
- [ ] PUT /users/me/status — update Redis hash, broadcast change
- [ ] Unit test: Redis read/write

## Phase 3 — WebSocket + Heartbeat
- [ ] WebSocket upgrade at /ws with JWT auth
- [ ] On connect: mark user online in Redis (TTL 90s), publish presence.updated
- [ ] Heartbeat ping handler: reset TTL
- [ ] On disconnect: mark offline, publish presence.updated
- [ ] Subscribe to Redis Pub/Sub, fan-out presence.updated to WS clients
- [ ] Integration test: connect → appear online → disconnect → appear offline

## Phase 4 — Typing Indicator
- [ ] POST /channels/{id}/typing — add user to Redis typing set (TTL 5s)
- [ ] Publish typing.started event to Redis
- [ ] Fan-out to channel WebSocket subscribers
- [ ] Integration test

## Phase 5 — Health & Docker
- [ ] GET /health
- [ ] Validate in Docker Compose

## Checkpoints
- ✅ Phase 1: service boots, Redis connected
- ✅ Phase 2: presence CRUD in Redis works
- ✅ Phase 3: real-time online/offline detection works
- ✅ Phase 4: typing indicators fire and expire
- ✅ Phase 5: healthy in Docker Compose

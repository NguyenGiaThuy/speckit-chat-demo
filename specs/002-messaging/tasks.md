# Tasks: Real-Time Messaging Service

## Phase 1 — Project Scaffolding
- [ ] Create `services/MessagingService` .NET 9 minimal API project
- [ ] Add NuGet packages: EF Core 9, Npgsql, StackExchange.Redis
- [ ] Configure PostgreSQL DbContext
- [ ] Add Message entity + migrations
- [ ] Add Dockerfile and docker-compose entry

## Phase 2 — REST Endpoints
- [ ] POST /channels/{channelId}/messages — create message, validate content
- [ ] GET /channels/{channelId}/messages — cursor-based pagination
- [ ] PUT /messages/{id} — edit (author check)
- [ ] DELETE /messages/{id} — soft delete (author check)
- [ ] Integration tests for all endpoints

## Phase 3 — Redis Pub/Sub
- [ ] Implement `RedisPubSubService`: publish message event to Redis
- [ ] Implement subscriber: listen to Redis, push to WebSocket manager
- [ ] Unit test: publish → receive round-trip

## Phase 4 — WebSocket
- [ ] Implement WebSocket upgrade endpoint at /ws
- [ ] JWT auth on WebSocket connection (query param)
- [ ] Implement `WebSocketManager`: register/unregister connections per channel
- [ ] Fan-out: on Redis event, send to all subscribed WebSocket clients
- [ ] Handle disconnect/reconnect gracefully
- [ ] Integration test: connect, subscribe, send message, receive real-time event

## Phase 5 — Health & Docker
- [ ] GET /health endpoint
- [ ] Validate full flow in Docker Compose (REST + WebSocket + Redis)

## Checkpoints
- ✅ Phase 1: service boots, DB migrated
- ✅ Phase 2: REST CRUD works with auth
- ✅ Phase 3: Redis pub/sub round-trip verified
- ✅ Phase 4: WebSocket delivers messages in real-time
- ✅ Phase 5: healthy in Docker Compose

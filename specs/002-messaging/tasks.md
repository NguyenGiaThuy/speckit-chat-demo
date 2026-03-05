# Tasks: Real-Time Messaging Service

## Phase 1 — Project Scaffolding
- [X] Create `services/MessagingService` .NET 9 minimal API project
- [X] Add NuGet packages: EF Core 9, Npgsql, StackExchange.Redis
- [X] Configure PostgreSQL DbContext
- [X] Add Message entity + migrations
- [X] Add Dockerfile and docker-compose entry

## Phase 2 — REST Endpoints
- [X] POST /channels/{channelId}/messages — create message, validate content
- [X] GET /channels/{channelId}/messages — cursor-based pagination
- [X] PUT /messages/{id} — edit (author check)
- [X] DELETE /messages/{id} — soft delete (author check)
- [X] Integration tests for all endpoints

## Phase 3 — Redis Pub/Sub
- [X] Implement `RedisPubSubService`: publish message event to Redis
- [X] Implement subscriber: listen to Redis, push to WebSocket manager
- [X] Unit test: publish → receive round-trip

## Phase 4 — WebSocket
- [X] Implement WebSocket upgrade endpoint at /ws
- [X] JWT auth on WebSocket connection (query param)
- [X] Implement `WebSocketManager`: register/unregister connections per channel
- [X] Fan-out: on Redis event, send to all subscribed WebSocket clients
- [X] Handle disconnect/reconnect gracefully
- [X] Integration test: connect, subscribe, send message, receive real-time event

## Phase 5 — Health & Docker
- [X] GET /health endpoint
- [X] Validate full flow in Docker Compose (REST + WebSocket + Redis)

## Checkpoints
- ✅ Phase 1: service boots, DB migrated
- ✅ Phase 2: REST CRUD works with auth
- ✅ Phase 3: Redis pub/sub round-trip verified
- ✅ Phase 4: WebSocket delivers messages in real-time
- ✅ Phase 5: healthy in Docker Compose

# Tasks: Notification Service

## Phase 1 — Scaffolding
- [ ] Create `services/NotificationService` .NET 9 minimal API
- [ ] Add EF Core 9, Npgsql, StackExchange.Redis
- [ ] Notification entity + migration
- [ ] Dockerfile and docker-compose entry

## Phase 2 — REST Endpoints
- [ ] GET /notifications — paginated list (cursor-based), filter unread
- [ ] PUT /notifications/{id}/read — mark single read
- [ ] PUT /notifications/read-all — mark all read
- [ ] Integration tests

## Phase 3 — Redis Event Listener
- [ ] Implement `EventListenerService` as IHostedService
- [ ] Subscribe to `events.mention` and `events.dm` Redis channels
- [ ] On event: create Notification in DB, push to WebSocket manager
- [ ] Unit test: event parsing and DB write

## Phase 4 — WebSocket
- [ ] WebSocket upgrade at /ws with JWT auth
- [ ] Register connection in WebSocketManager keyed by user_id
- [ ] On new notification: send to connected user's WebSocket
- [ ] Integration test: trigger event → WebSocket client receives notification

## Phase 5 — Health & Docker
- [ ] GET /health
- [ ] Validate full flow in Docker Compose

## Checkpoints
- ✅ Phase 1: service boots, DB migrated
- ✅ Phase 2: REST notification management works
- ✅ Phase 3: Redis events trigger DB records
- ✅ Phase 4: real-time delivery via WebSocket works
- ✅ Phase 5: healthy in Docker Compose

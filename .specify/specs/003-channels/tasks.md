# Tasks: Guild & Channel Service

## Phase 1 — Scaffolding
- [ ] Create `services/GuildService` .NET 9 minimal API project
- [ ] Add EF Core 9, Npgsql
- [ ] Add Guild, Channel, Member, GuildInvite entities + migrations
- [ ] Dockerfile and docker-compose entry

## Phase 2 — Guild Endpoints
- [ ] POST /guilds — create guild, add creator as member+owner
- [ ] GET /guilds — list guilds for authenticated user
- [ ] GET /guilds/{id} — get guild details (member only)
- [ ] Integration tests

## Phase 3 — Channel Endpoints
- [ ] POST /guilds/{id}/channels — create channel (owner only)
- [ ] GET /guilds/{id}/channels — list channels (member only)
- [ ] PUT /channels/{id} — update (owner only)
- [ ] DELETE /channels/{id} — delete with cascade (owner only)
- [ ] Integration tests

## Phase 4 — Invite System
- [ ] POST /guilds/{id}/invites — generate token (24h TTL)
- [ ] POST /guilds/join — validate token, add member
- [ ] Unit test: token expiry and idempotency

## Phase 5 — Health & Docker
- [ ] GET /health
- [ ] Validate in Docker Compose

## Checkpoints
- ✅ Phase 1: service boots, migrations applied
- ✅ Phase 2: guild CRUD works
- ✅ Phase 3: channel management works with auth
- ✅ Phase 4: invite + join flow works end-to-end
- ✅ Phase 5: healthy in Docker Compose

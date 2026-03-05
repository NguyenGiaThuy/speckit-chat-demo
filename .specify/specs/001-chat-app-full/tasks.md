# Tasks: Full Chat App (branch-01)

> ⚠️ This is what happens when you create tasks for an entire application at once.
> 80+ tasks, unclear ownership, high chance of blocking dependencies.

## Phase 1 — All Infrastructure Setup
- [ ] Set up Docker Compose with all 7 services + postgres + redis + livekit
- [ ] Create all PostgreSQL databases and run all migrations
- [ ] Set up YARP API gateway
- [ ] Configure JWT RS256 keys for all services
- [ ] Create shared OpenAPI contracts package

## Phase 2 — Identity Service (everything)
- [ ] Register, login, refresh, logout endpoints
- [ ] Profile endpoints
- [ ] 2FA (TOTP) setup and verification
- [ ] User ban/unban (admin)
- [ ] OAuth2 integration (future prep)

## Phase 3 — Guild Service (everything)
- [ ] Guild CRUD
- [ ] Channel CRUD (text, voice, announcement)
- [ ] Channel categories
- [ ] Role CRUD
- [ ] Permission bitmask system
- [ ] Per-channel permission overrides
- [ ] Invite link system
- [ ] Member management

## Phase 4 — Messaging Service (everything)
- [ ] Send/edit/delete text messages
- [ ] File/image upload (S3 or local)
- [ ] Emoji reactions
- [ ] Message pinning
- [ ] Threaded replies
- [ ] Direct messages
- [ ] Read receipts (DMs)
- [ ] Message search
- [ ] WebSocket real-time delivery

## Phase 5 — Presence Service
- [ ] Online/offline via Redis TTL
- [ ] Manual status
- [ ] Typing indicators
- [ ] WebSocket events

## Phase 6 — Notification Service
- [ ] Mention notifications
- [ ] DM notifications
- [ ] Notification settings per guild/channel
- [ ] Desktop push (Web Push API)

## Phase 7 — Voice Service
- [ ] LiveKit room creation per voice channel
- [ ] Join/leave voice channel
- [ ] Screen share
- [ ] Mute/deafen/video toggle

## Phase 8 — Admin Service
- [ ] Audit log
- [ ] Message moderation
- [ ] Report handling
- [ ] Guild admin settings

## Checkpoints
- ✅ All services boot in Docker Compose (no individual validation possible)
- ✅ End-to-end test (requires all services complete)

> At this point it becomes clear why this approach is risky:
> You cannot validate anything until everything is built.
> Compare with feature branches where each phase has its own checkpoint.

# Spec: Guild & Channel Management

## Feature Name
003-channels

## Overview
Guild service handling creation and management of guilds (servers) and their channels.
Depends on feature/01-user-auth for identity.

## User Stories

- **US-01**: As an authenticated user, I can create a guild
- **US-02**: As a guild owner, I can create text or voice channels inside the guild
- **US-03**: As a guild owner, I can update or delete channels
- **US-04**: As a guild owner, I can invite others via an invite link
- **US-05**: As a user, I can join a guild via an invite link
- **US-06**: As a user, I can list guilds I belong to and their channels

## Acceptance Criteria

### US-01 Create Guild
- [ ] POST /guilds creates a guild, creator becomes owner and first member
- [ ] Name 2–100 chars, required
- [ ] Returns guild id, name, owner_id

### US-02 Create Channel
- [ ] POST /guilds/{id}/channels creates channel
- [ ] Type: text | voice
- [ ] Name 2–50 chars, unique within guild
- [ ] Only guild owner/admin can create channels

### US-03 Update/Delete Channel
- [ ] PUT /channels/{id} updates name or type
- [ ] DELETE /channels/{id} removes channel (and its messages via cascade)
- [ ] Only guild owner/admin allowed

### US-04 Invite Link
- [ ] POST /guilds/{id}/invites generates a short-lived invite token (24h TTL)
- [ ] Returns invite URL

### US-05 Join Guild
- [ ] POST /guilds/join with invite token adds user as member
- [ ] Expired/invalid tokens return 410 Gone
- [ ] Already-member returns 200 (idempotent)

### US-06 List
- [ ] GET /guilds returns guilds the authenticated user belongs to
- [ ] GET /guilds/{id}/channels returns channels for the guild (member only)

## Data Model

```
Guild:         id, name, owner_id, icon_url, created_at
Channel:       id, guild_id, name, type(text|voice), position, created_at
Member:        guild_id, user_id, joined_at (composite PK)
GuildInvite:   id, guild_id, created_by, token, expires_at, used_count
```

## API Contracts

```
POST   /guilds                          → 201 Guild
GET    /guilds                          → 200 Guild[]
GET    /guilds/{id}                     → 200 Guild
POST   /guilds/{id}/channels            → 201 Channel
GET    /guilds/{id}/channels            → 200 Channel[]
PUT    /channels/{id}                   → 200 Channel
DELETE /channels/{id}                   → 204
POST   /guilds/{id}/invites             → 201 { invite_url }
POST   /guilds/join  { token }          → 200
```

## Architecture Notes
- Service: `GuildService` (.NET 9, port 8082)
- DB: PostgreSQL (guild_db)
- Depends on: identity-service (JWT validation)

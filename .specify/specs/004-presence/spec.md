# Spec: User Presence Service

## Feature Name
004-presence

## Overview
Presence microservice tracking online/offline/away status per user using Redis.
Publishes real-time presence events over WebSocket.

## User Stories

- **US-01**: As a user, I can see which guild members are currently online
- **US-02**: As a user, my status updates to online when I connect and offline when I disconnect
- **US-03**: As a user, I can manually set my status (online, away, do not disturb, invisible)
- **US-04**: As a user, I can see typing indicators when someone is typing in a channel

## Acceptance Criteria

### US-01 Member Online Status
- [ ] GET /guilds/{id}/presence returns list of member statuses
- [ ] Status values: online | away | dnd | offline
- [ ] Invisible users appear as offline to others

### US-02 Auto Presence
- [ ] When WebSocket connects (authenticated), user marked online in Redis (TTL 90s)
- [ ] Heartbeat ping every 60s resets TTL
- [ ] On disconnect or TTL expiry, user marked offline
- [ ] Presence change broadcast to guild members via Redis Pub/Sub

### US-03 Manual Status
- [ ] PUT /users/me/status accepts status + optional message
- [ ] Persists in Redis, broadcasts change
- [ ] Invisible: stored as invisible in Redis, exposed as offline to others

### US-04 Typing Indicator
- [ ] POST /channels/{id}/typing triggers typing event
- [ ] Event broadcast to channel subscribers via Redis Pub/Sub
- [ ] Typing indicator auto-clears after 5s (no stop event needed)

## Data Model (Redis keys)

```
presence:user:{userId}          → Hash { status, status_message, last_seen }  TTL 90s
presence:guild:{guildId}:online → Set of userIds currently online
typing:channel:{channelId}      → Set of userIds (TTL 5s per member)
```

## API Contracts

```
GET  /guilds/{id}/presence         → 200 [{ user_id, status, status_message }]
PUT  /users/me/status              → 200 { status, status_message }
     Body: { status, status_message? }
POST /channels/{id}/typing         → 204

WebSocket: ws://presence-service/ws
  Auth: ?token=<jwt>
  Events received by client:
    { type: "presence.updated", data: { user_id, status } }
    { type: "typing.started",   data: { user_id, channel_id } }
```

## Architecture Notes
- Service: `PresenceService` (.NET 9, port 8083)
- State: Redis only (no PostgreSQL needed)
- Depends on: identity-service (JWT), guild-service (member lists)

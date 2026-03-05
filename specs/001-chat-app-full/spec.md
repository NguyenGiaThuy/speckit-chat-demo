# Spec: Chat Application — Full (branch-01)

> ⚠️ This branch demonstrates specifying the **entire application at once**.
> This results in a massive, hard-to-review spec with many dependencies and unclear priorities.

## Feature Name
001-chat-app-full

## Overview
Build a full Discord-like chat application with user authentication, real-time messaging, guild/channel management, presence tracking, notifications, voice/video chat, file uploads, emoji reactions, role-based permissions, audit logs, and admin dashboard.

---

## User Stories

### Authentication & Identity
- US-01: As a user, I can register with email and password
- US-02: As a user, I can log in and receive a JWT token
- US-03: As a user, I can refresh my token
- US-04: As a user, I can log out and revoke my token
- US-05: As a user, I can update my profile (avatar, display name, bio)
- US-06: As an admin, I can ban/unban users
- US-07: As a user, I can enable 2FA on my account

### Guilds & Channels
- US-08: As a user, I can create a guild
- US-09: As a user, I can invite others to my guild via invite link
- US-10: As a guild owner, I can create text, voice, and announcement channels
- US-11: As a guild owner, I can create channel categories
- US-12: As a guild owner, I can set per-channel permissions per role
- US-13: As a user, I can join a guild via invite link
- US-14: As a user, I can leave a guild
- US-15: As a guild owner, I can delete the guild

### Messaging
- US-16: As a user, I can send a text message in a channel
- US-17: As a user, I can edit my own messages
- US-18: As a user, I can delete my own messages
- US-19: As a user, I can reply to a message (threaded replies)
- US-20: As a user, I can react to a message with an emoji
- US-21: As a user, I can pin a message in a channel
- US-22: As a user, I can upload files and images
- US-23: As a user, I can send a direct message to another user
- US-24: As a user, I can see message read receipts in DMs
- US-25: As a user, I can search messages within a channel

### Presence & Status
- US-26: As a user, I can see who is online in my guild
- US-27: As a user, I can set my status (online, away, do not disturb, invisible)
- US-28: As a user, I can see typing indicators in channels

### Notifications
- US-29: As a user, I receive notifications for mentions
- US-30: As a user, I can configure notification settings per guild and channel
- US-31: As a user, I receive desktop push notifications

### Voice & Video
- US-32: As a user, I can join a voice channel
- US-33: As a user, I can share my screen in a voice channel
- US-34: As a user, I can mute/unmute and deafen/undeafen
- US-35: As a user, I can enable/disable video

### Roles & Permissions
- US-36: As a guild owner, I can create roles with custom permissions
- US-37: As a guild owner, I can assign roles to members
- US-38: Permissions cascade: guild > category > channel

### Admin & Audit
- US-39: As an admin, I can view the audit log
- US-40: As an admin, I can configure guild-wide settings
- US-41: As a user, I can report messages for moderation

---

## Acceptance Criteria
*(Too numerous to be practical — see individual feature specs for tractable acceptance criteria)*

- All services deployed via Docker Compose
- JWT authentication enforced on all protected endpoints
- Real-time features use LiveKit (voice/video) and WebSocket (messaging/presence)
- Redis for caching and pub/sub
- All services return structured error responses
- Health check endpoints on all services

---

## Data Models (abbreviated)

### User
- id, email, password_hash, display_name, avatar_url, bio, created_at, updated_at

### Guild
- id, name, owner_id, icon_url, created_at

### Channel
- id, guild_id, name, type (text/voice/announcement), category_id, position, permissions

### Message
- id, channel_id, author_id, content, attachments[], reactions[], reply_to_id, created_at, edited_at, pinned

### Member
- guild_id, user_id, roles[], joined_at, nickname

### Role
- id, guild_id, name, permissions_bitmask, color, position

### DirectMessage
- id, participants[], messages[]

### Presence
- user_id, status, last_seen, typing_in_channel_id

### Notification
- id, user_id, type, payload, read, created_at

---

## API Contracts (abbreviated)

- `POST /auth/register`
- `POST /auth/login`
- `POST /auth/refresh`
- `POST /auth/logout`
- `GET/PUT /users/{id}`
- `POST /guilds`
- `GET /guilds/{id}`
- `DELETE /guilds/{id}`
- `POST /guilds/{id}/channels`
- `GET /guilds/{id}/channels`
- `POST /channels/{id}/messages`
- `GET /channels/{id}/messages`
- `PUT /messages/{id}`
- `DELETE /messages/{id}`
- `POST /messages/{id}/reactions`
- `POST /users/{id}/dm`
- `GET /guilds/{id}/members`
- `POST /guilds/{id}/roles`
- `PUT /guilds/{id}/members/{userId}/roles`
- `WebSocket /ws/messaging`
- `WebSocket /ws/presence`
- `LiveKit /livekit/join`
- `GET /notifications`
- `PUT /notifications/{id}/read`
- `GET /audit-log`
- *(and ~30 more endpoints)*

---

## Review & Acceptance Checklist
- [ ] All user stories addressed
- [ ] Data models cover all entities
- [ ] API contracts defined for all services
- [ ] Auth flow documented
- [ ] Real-time protocol specified
- [ ] Role/permission system defined
- [ ] Docker Compose topology documented

---

> **Note for reviewers:** This spec is intentionally overwhelming.
> With 41 user stories across 7 domains, 10+ data models, and 30+ endpoints, 
> no single developer can review or implement this safely.
> Compare with the `feature/*` branches where each spec covers 3-6 user stories.

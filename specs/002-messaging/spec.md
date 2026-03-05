# Spec: Real-Time Messaging Service

## Feature Name
002-messaging

## Overview
Messaging microservice handling sending, editing, deleting messages in channels,
with real-time delivery over WebSocket. Depends on feature/01-user-auth for auth.

## User Stories

- **US-01**: As an authenticated user, I can send a text message to a channel
- **US-02**: As an authenticated user, I can edit my own messages
- **US-03**: As an authenticated user, I can delete my own messages
- **US-04**: As an authenticated user, I can view message history in a channel (paginated)
- **US-05**: As an authenticated user, I receive new messages in real-time via WebSocket
- **US-06**: As an authenticated user, I can reply to a message (thread reference)

## Acceptance Criteria

### US-01 Send Message
- [ ] POST /channels/{id}/messages creates a new message
- [ ] Message content max 2000 characters
- [ ] Empty messages rejected (422)
- [ ] Message is broadcast in real-time to all connected channel subscribers

### US-02 Edit Message
- [ ] PUT /messages/{id} updates message content
- [ ] Only message author can edit (403 for others)
- [ ] Edit timestamp recorded
- [ ] Real-time broadcast of edit to channel subscribers

### US-03 Delete Message
- [ ] DELETE /messages/{id} soft-deletes the message
- [ ] Only author can delete own messages (403 for others)
- [ ] Real-time broadcast of deletion to channel subscribers

### US-04 Message History
- [ ] GET /channels/{id}/messages returns paginated list (cursor-based, 50 per page)
- [ ] Messages ordered newest first
- [ ] Deleted messages shown as "[message deleted]"

### US-05 Real-Time
- [ ] WebSocket connection authenticated via JWT (query param or header)
- [ ] Client subscribes to channel(s) on connect
- [ ] Events: message.created, message.updated, message.deleted
- [ ] Missed messages on reconnect delivered via REST history

### US-06 Reply
- [ ] reply_to_id field references parent message
- [ ] Parent message snippet included in response
- [ ] Reply displayed inline in channel (not a thread — just a reference)

## Data Model

### Message
```
id          GUID PK
channel_id  GUID FK
author_id   GUID FK (user)
content     string (max 2000)
reply_to_id GUID nullable FK -> Message
deleted     bool default false
created_at  datetime
edited_at   datetime nullable
```

## API Contracts

```
POST /channels/{channelId}/messages
  Headers: Authorization: Bearer <token>
  Body: { content, reply_to_id? }
  201: { id, channel_id, author_id, content, reply_to_id, created_at }
  422: validation error

GET /channels/{channelId}/messages?cursor=<id>&limit=50
  Headers: Authorization: Bearer <token>
  200: { messages: [...], next_cursor }

PUT /messages/{id}
  Headers: Authorization: Bearer <token>
  Body: { content }
  200: { id, content, edited_at }
  403: not author
  404: not found

DELETE /messages/{id}
  Headers: Authorization: Bearer <token>
  204
  403: not author

WebSocket: ws://messaging-service/ws
  Auth: ?token=<jwt>
  Subscribe: { action: "subscribe", channel_id }
  Events:
    { type: "message.created", data: Message }
    { type: "message.updated", data: { id, content, edited_at } }
    { type: "message.deleted", data: { id } }
```

## Architecture Notes
- Service: `MessagingService` (.NET 9 minimal API + System.Net.WebSockets)
- DB: PostgreSQL
- Redis Pub/Sub: publish events to Redis → fan-out to all WebSocket subscribers
- Docker Compose service: `messaging-service:8081`
- Depends on: `identity-service` (JWT validation)

## Out of Scope
- File/image attachments (future)
- Emoji reactions (future)
- Message search (future)

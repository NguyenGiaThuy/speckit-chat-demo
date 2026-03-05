# Spec: Notification Service

## Feature Name
005-notifications

## Overview
Notification microservice that delivers in-app notifications for mentions and direct messages.
Listens to events from other services via Redis Pub/Sub.

## User Stories

- **US-01**: As a user, I receive a notification when someone @mentions me in a channel
- **US-02**: As a user, I receive a notification when I get a direct message
- **US-03**: As a user, I can list my unread notifications
- **US-04**: As a user, I can mark notifications as read (individually or all at once)
- **US-05**: As a user, I receive notifications in real-time via WebSocket

## Acceptance Criteria

### US-01 Mention Notification
- [ ] MessagingService publishes `message.mention` event when message contains @userId
- [ ] NotificationService receives event, creates notification record
- [ ] Notification pushed in real-time to mentioned user's WebSocket connection

### US-02 DM Notification
- [ ] MessagingService publishes `dm.received` event on new DM
- [ ] Notification created and pushed to recipient in real-time

### US-03 List Notifications
- [ ] GET /notifications returns paginated notifications (newest first, 20 per page)
- [ ] Includes: id, type, payload, read, created_at
- [ ] Filter: ?unread=true

### US-04 Mark Read
- [ ] PUT /notifications/{id}/read marks single notification read
- [ ] PUT /notifications/read-all marks all as read
- [ ] Returns 204

### US-05 Real-Time Delivery
- [ ] WebSocket at /ws delivers notifications as they arrive
- [ ] Event: { type: "notification.new", data: Notification }

## Data Model

### Notification
```
id          GUID PK
user_id     GUID FK
type        enum (mention | dm)
payload     jsonb  { guild_id?, channel_id?, message_id?, from_user_id }
read        bool default false
created_at  datetime
```

## API Contracts

```
GET  /notifications?unread=true&cursor=<id>
     → 200 { notifications: [...], next_cursor }

PUT  /notifications/{id}/read   → 204
PUT  /notifications/read-all    → 204

WebSocket: ws://notification-service/ws
  Auth: ?token=<jwt>
  Events:
    { type: "notification.new", data: Notification }
```

## Redis Events Consumed
```
channel: "events.mention"   payload: { user_id, guild_id, channel_id, message_id, from_user_id }
channel: "events.dm"        payload: { user_id, from_user_id, message_id }
```

## Architecture Notes
- Service: `NotificationService` (.NET 9, port 8084)
- DB: PostgreSQL (notification_db)
- Depends on: Redis (events), identity-service (JWT)

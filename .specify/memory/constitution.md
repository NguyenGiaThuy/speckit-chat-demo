# Project Constitution

## Project Vision
A Discord-like chat application built as .NET 9 microservices, demonstrating Spec-Driven Development.

## Core Principles
1. **Small, focused specs** — Each feature is specified independently on its own branch
2. **Spec before code** — No implementation without an approved spec
3. **Microservice boundaries** — Each service owns its domain; no cross-service DB access
4. **Testability first** — Every feature spec includes acceptance criteria and test scenarios

## Architecture Constraints
- Runtime: .NET 9
- Messaging/RT: LiveKit for real-time communication; Redis Pub/Sub for internal events
- Cache/Presence: Redis
- Transport: REST for CRUD; WebSocket/gRPC for real-time
- Deployment: Docker Compose (dev); each service has its own Dockerfile
- Auth: JWT issued by Identity Service; all other services validate tokens

## Coding Standards
- One microservice per domain (identity, messaging, channels, presence, notifications)
- Shared contracts via a `contracts/` package (OpenAPI + protobuf)
- No shared databases between services
- All services expose health endpoints

## Spec Standards
- Every spec must include: user stories, acceptance criteria, data model, API contracts
- Feature branches must be based off `main`
- One feature per branch

# speckit-chat-demo

**Demo:** Spec-Driven Development with [spec-kit](https://github.com/github/spec-kit) — comparing two approaches:

- `branch-01`: entire chat app specified at once (❌ bloated, hard to parallelize)
- `feature/*` branches: each developer owns one small spec (✅ focused, reviewable, parallelizable)

## Stack
- .NET 9 microservices
- Redis (presence/cache)
- LiveKit (real-time voice/video/messaging)
- Docker Compose

## Feature Branches
| Branch | Feature |
|---|---|
| `feature/01-user-auth` | Identity service — register, login, JWT |
| `feature/02-messaging` | Real-time chat service (WebSocket/LiveKit) |
| `feature/03-channels` | Guild & channel management |
| `feature/04-presence` | Online/offline presence via Redis |
| `feature/05-notifications` | Push notification service |

## How to Read This Demo
1. Check `branch-01` — one massive spec for the whole app
2. Check any `feature/*` branch — a small, focused spec per developer
3. See how feature branches merge cleanly; branch-01 would require full re-work

## Setup
```bash
uv tool install specify-cli --from git+https://github.com/github/spec-kit.git
specify init . --here --force --ai copilot
```

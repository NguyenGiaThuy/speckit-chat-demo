# Tasks: User Authentication (Identity Service)

## Phase 1 — Project Scaffolding
- [X] Create `services/IdentityService` .NET 9 minimal API project
- [X] Add NuGet packages: EF Core 9, Npgsql, BCrypt.Net, Microsoft.IdentityModel.Tokens
- [X] Configure PostgreSQL DbContext
- [X] Add User and RefreshToken entities + migrations
- [X] Add Dockerfile and docker-compose entry

## Phase 2 — Token Service
- [X] Implement `TokenService`: generate RS256 JWT access token
- [X] Implement `TokenService`: generate + hash refresh token
- [X] Implement `TokenService`: validate + rotate refresh token
- [X] Unit test: token generation and validation

## Phase 3 — Auth Endpoints
- [X] POST /auth/register — hash password, create user, return 201
- [X] POST /auth/login — verify password, issue token pair
- [X] POST /auth/refresh — rotate refresh token, return new access token
- [X] POST /auth/logout — revoke refresh token
- [X] Integration test: full auth flow

## Phase 4 — User Profile Endpoints
- [X] GET /users/me — return profile from JWT claims
- [X] PUT /users/me — update display_name, avatar_url
- [X] Unit test: profile CRUD

## Phase 5 — Health & Docker
- [X] Add GET /health endpoint
- [X] Validate Docker Compose startup
- [X] Verify JWT can be validated by a second service using public key

## Checkpoints
- ✅ Phase 1: service boots and connects to DB
- ✅ Phase 2: tokens round-trip correctly in unit tests
- ✅ Phase 3: register → login → refresh → logout flow works end-to-end
- ✅ Phase 4: authenticated profile reads and updates work
- ✅ Phase 5: service healthy in Docker Compose

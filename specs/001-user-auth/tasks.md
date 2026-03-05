# Tasks: User Authentication (Identity Service)

## Phase 1 — Project Scaffolding
- [ ] Create `services/IdentityService` .NET 9 minimal API project
- [ ] Add NuGet packages: EF Core 9, Npgsql, BCrypt.Net, Microsoft.IdentityModel.Tokens
- [ ] Configure PostgreSQL DbContext
- [ ] Add User and RefreshToken entities + migrations
- [ ] Add Dockerfile and docker-compose entry

## Phase 2 — Token Service
- [ ] Implement `TokenService`: generate RS256 JWT access token
- [ ] Implement `TokenService`: generate + hash refresh token
- [ ] Implement `TokenService`: validate + rotate refresh token
- [ ] Unit test: token generation and validation

## Phase 3 — Auth Endpoints
- [ ] POST /auth/register — hash password, create user, return 201
- [ ] POST /auth/login — verify password, issue token pair
- [ ] POST /auth/refresh — rotate refresh token, return new access token
- [ ] POST /auth/logout — revoke refresh token
- [ ] Integration test: full auth flow

## Phase 4 — User Profile Endpoints
- [ ] GET /users/me — return profile from JWT claims
- [ ] PUT /users/me — update display_name, avatar_url
- [ ] Unit test: profile CRUD

## Phase 5 — Health & Docker
- [ ] Add GET /health endpoint
- [ ] Validate Docker Compose startup
- [ ] Verify JWT can be validated by a second service using public key

## Checkpoints
- ✅ Phase 1: service boots and connects to DB
- ✅ Phase 2: tokens round-trip correctly in unit tests
- ✅ Phase 3: register → login → refresh → logout flow works end-to-end
- ✅ Phase 4: authenticated profile reads and updates work
- ✅ Phase 5: service healthy in Docker Compose

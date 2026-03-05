# Spec: User Authentication (Identity Service)

## Feature Name
001-user-auth

## Overview
Identity microservice handling user registration, login, JWT issuance, and token refresh.
This is the foundation all other services depend on for authentication.

## User Stories

- **US-01**: As a new user, I can register with email and password so I can access the app
- **US-02**: As a registered user, I can log in and receive a JWT access token + refresh token
- **US-03**: As an authenticated user, I can refresh my access token without re-logging in
- **US-04**: As an authenticated user, I can log out and revoke my refresh token
- **US-05**: As an authenticated user, I can view and update my profile (display name, avatar URL)

## Acceptance Criteria

### US-01 Register
- [ ] POST /auth/register accepts email + password
- [ ] Password must be ≥8 chars, at least one uppercase, one number
- [ ] Duplicate emails return 409 Conflict
- [ ] Password is hashed (bcrypt), never stored plain
- [ ] Returns 201 with user id and display name

### US-02 Login
- [ ] POST /auth/login accepts email + password
- [ ] Returns access token (15 min TTL) + refresh token (7 day TTL)
- [ ] Wrong credentials return 401
- [ ] Tokens are JWTs signed with RS256

### US-03 Refresh
- [ ] POST /auth/refresh accepts refresh token
- [ ] Returns new access token
- [ ] Expired/invalid refresh token returns 401
- [ ] Refresh token is rotated (old one invalidated)

### US-04 Logout
- [ ] POST /auth/logout invalidates refresh token
- [ ] Subsequent refresh with old token returns 401

### US-05 Profile
- [ ] GET /users/me returns current user profile
- [ ] PUT /users/me updates display_name and avatar_url
- [ ] Requires valid access token

## Data Model

### User
```
id            GUID PK
email         string UNIQUE NOT NULL
password_hash string NOT NULL
display_name  string NOT NULL
avatar_url    string nullable
created_at    datetime
updated_at    datetime
```

### RefreshToken
```
id         GUID PK
user_id    GUID FK -> User
token_hash string UNIQUE
expires_at datetime
revoked    bool
created_at datetime
```

## API Contracts

```
POST /auth/register
  Body: { email, password, display_name }
  201: { id, display_name }
  409: { error: "email_taken" }
  422: { error: "validation_error", details }

POST /auth/login
  Body: { email, password }
  200: { access_token, refresh_token, expires_in }
  401: { error: "invalid_credentials" }

POST /auth/refresh
  Body: { refresh_token }
  200: { access_token, refresh_token, expires_in }
  401: { error: "invalid_token" }

POST /auth/logout
  Headers: Authorization: Bearer <token>
  Body: { refresh_token }
  204

GET /users/me
  Headers: Authorization: Bearer <token>
  200: { id, email, display_name, avatar_url, created_at }

PUT /users/me
  Headers: Authorization: Bearer <token>
  Body: { display_name?, avatar_url? }
  200: { id, email, display_name, avatar_url }
```

## Architecture Notes
- Service: `IdentityService` (.NET 9 minimal API)
- DB: PostgreSQL
- JWT: RS256, keys managed via env vars
- Other services validate tokens by checking signature with public key
- Docker Compose service: `identity-service:8080`

## Out of Scope (handled in other features)
- 2FA (future)
- OAuth/SSO (future)
- Role assignment (see feature/03-channels)

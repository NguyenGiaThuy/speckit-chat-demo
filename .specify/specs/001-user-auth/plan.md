# Implementation Plan: User Authentication (Identity Service)

## Tech Stack
- Runtime: .NET 9 minimal API
- Database: PostgreSQL (via EF Core 9)
- Auth: JWT RS256 (Microsoft.IdentityModel.Tokens)
- Password hashing: BCrypt.Net
- Container: Docker (port 8080)

## Architecture

```
IdentityService/
├── Endpoints/
│   ├── AuthEndpoints.cs       (register, login, refresh, logout)
│   └── UserEndpoints.cs       (GET/PUT /users/me)
├── Services/
│   ├── AuthService.cs
│   ├── TokenService.cs
│   └── UserService.cs
├── Data/
│   ├── AppDbContext.cs
│   ├── Entities/User.cs
│   └── Entities/RefreshToken.cs
├── DTOs/
│   ├── RegisterRequest.cs
│   ├── LoginRequest.cs
│   └── UserProfileDto.cs
├── Dockerfile
└── Program.cs
```

## Service Dependencies
- PostgreSQL (identity_db)
- No other services

## Environment Variables
```
DB_CONNECTION_STRING=Host=postgres;Database=identity_db;...
JWT_PRIVATE_KEY=<RS256 private key PEM>
JWT_PUBLIC_KEY=<RS256 public key PEM>
JWT_ISSUER=speckit-chat
JWT_ACCESS_TTL_MINUTES=15
JWT_REFRESH_TTL_DAYS=7
```

## Docker Compose Entry
```yaml
identity-service:
  build: ./services/IdentityService
  ports:
    - "8080:8080"
  environment:
    - DB_CONNECTION_STRING
    - JWT_PRIVATE_KEY
    - JWT_PUBLIC_KEY
  depends_on:
    - postgres
```

## Research Notes
- EF Core 9 with Npgsql provider for PostgreSQL
- BCrypt cost factor 12 (balances security/speed)
- RS256 preferred over HS256: other services can validate with public key only
- Refresh token stored as hash, not plain value

using MessagingService.Data;
using MessagingService.Endpoints;
using MessagingService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ── PostgreSQL ──────────────────────────────────────────────────────────────
var connectionString = builder.Configuration["DB_CONNECTION_STRING"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DB_CONNECTION_STRING is not configured");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ── Redis ────────────────────────────────────────────────────────────────────
var redisConn = builder.Configuration["REDIS_CONNECTION"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConn));

// ── JWT ──────────────────────────────────────────────────────────────────────
var jwtPublicKeyPem = builder.Configuration["JWT_PUBLIC_KEY"]
    ?? throw new InvalidOperationException("JWT_PUBLIC_KEY is not configured");
jwtPublicKeyPem = jwtPublicKeyPem.Replace("\\n", "\n");
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "speckit-chat";

var rsa = System.Security.Cryptography.RSA.Create();
rsa.ImportFromPem(jwtPublicKeyPem);
var jwtPublicKey = new RsaSecurityKey(rsa);

var tokenValidationParams = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = jwtIssuer,
    ValidateAudience = false,
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = jwtPublicKey,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero
};

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = tokenValidationParams);
builder.Services.AddAuthorization();

// ── App services ─────────────────────────────────────────────────────────────
builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<RedisPubSubService>();
builder.Services.AddSingleton<ITokenValidator>(_ => new JwtTokenValidator(tokenValidationParams));
builder.Services.AddScoped<MessageService>();

var app = builder.Build();

// ── DB migration ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ── Start Redis subscriber ───────────────────────────────────────────────────
var pubSub = app.Services.GetRequiredService<RedisPubSubService>();
await pubSub.StartSubscribingAsync();

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();

// ── Endpoints ─────────────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
app.MapMessageEndpoints();
app.MapWebSocketEndpoint();

app.Run();

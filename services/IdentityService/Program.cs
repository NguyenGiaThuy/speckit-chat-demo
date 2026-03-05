using IdentityService.Data;
using IdentityService.Endpoints;
using IdentityService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL DbContext
var connectionString = builder.Configuration["DB_CONNECTION_STRING"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DB_CONNECTION_STRING is not configured");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Services
builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();

// JWT Authentication - configure directly from IConfiguration to avoid BuildServiceProvider
var jwtPublicKeyPem = builder.Configuration["JWT_PUBLIC_KEY"]
    ?? throw new InvalidOperationException("JWT_PUBLIC_KEY is not configured");
// Docker/shell env vars encode newlines as literal \n — restore them for PEM parsing
jwtPublicKeyPem = jwtPublicKeyPem.Replace("\\n", "\n");
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "speckit-chat";

var rsaPublic = System.Security.Cryptography.RSA.Create();
rsaPublic.ImportFromPem(jwtPublicKeyPem);
var jwtPublicKey = new RsaSecurityKey(rsaPublic);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtPublicKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Feature endpoints
app.MapAuthEndpoints();
app.MapUserEndpoints();

app.Run();

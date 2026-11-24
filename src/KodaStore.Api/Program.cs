using KodaStore.Api.Middlewares;
using Scalar.AspNetCore;
using KodaStore.Infrastructure;
using KodaStore.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using KodaStore.Infrastructure.Seeding;
using Microsoft.Extensions.Options;
using KodaStore.Application.Security;
using KodaStore.Application;

var builder = WebApplication.CreateBuilder(args);

// Membaca daftar origin di env.Development
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

// Register CORS Configuration
builder.Services.AddCors(option =>
{
    option.AddPolicy("ReactClient", policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

// Scalar
builder.Services.AddOpenApi();

// DbContext
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var jwtOption = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? throw new InvalidOperationException("JWT Configuration not available.");

if (string.IsNullOrWhiteSpace(jwtOption.Issuer)) throw new InvalidOperationException("Jwt: Issuer is required.");

if (string.IsNullOrWhiteSpace(jwtOption.Audience)) throw new InvalidOperationException("Jwt: Audience is required.");

if (jwtOption.AccessTokenMinutes is < 1 or > 60) throw new InvalidOperationException("Jwt: Access Token minutes between 1 - 60");

byte[] signingKeyBytes;
try
{
    signingKeyBytes = Convert.FromBase64String(jwtOption.SigningKeyBase64);
}
catch (FormatException e)
{
    throw new InvalidOperationException("Jwt: SigningKeyBase64 didnt use valid base64.", e);
}

// HS256
if (signingKeyBytes.Length < 32) throw new InvalidOperationException("JWT signing key minimum of 32 bytes");

builder.Services.AddControllersWithViews();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(
    option =>
    {
        option.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance ??= context.HttpContext.Request.Path.Value;
            context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        };
    }
);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddRateLimiter(option =>
{
    option.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    option.AddConcurrencyLimiter("category-writes", limiter =>
    {
        limiter.PermitLimit = 8;
        limiter.QueueLimit = 0;
    });

    option.AddPolicy("auth-login", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Nihil";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"login: {ip}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(3),
                QueueLimit = 0,
                AutoReplenishment = true
            }
        );
    });

    option.AddPolicy("auth-register", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Nihil";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"login: {ip}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }
        );
    });

    option.AddPolicy("auth-refresh", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Nihil";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"login: {ip}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(2),
                QueueLimit = 0,
                AutoReplenishment = true
            }
        );
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
{
    option.MapInboundClaims = false;
    option.IncludeErrorDetails = false;
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOption.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtOption.Audience,

        IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),
        RequireSignedTokens = true,
        ValidAlgorithms = [
                SecurityAlgorithms.HmacSha256
            ],

        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = JwtRegisteredClaimNames.Sub,
        RoleClaimType = "role"
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.CanManageCategory, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AppRoles.Admin);
    });

    options.AddPolicy(AppPolicies.CanManageProduct, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AppRoles.Admin);
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Service
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

    using var scope = app.Services.CreateScope();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    var bootstrapAdminEmail = app.Configuration["RoleBootstrap:AdminEmail"];

    await RoleSeedData.SeedAsync(roleManager, userManager, bootstrapAdminEmail);
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMiddleware<RequestTiming>();

app.UseCors("ReactClient");

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.UseAuthorization();

app.Run();


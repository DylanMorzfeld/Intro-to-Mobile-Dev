using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FitTrack.Api.Auth;
using FitTrack.Api.Data;
using FitTrack.Api.Repositories;
using FitTrack.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// "dotnet run -- --setup-auth": save a signing key and the demo user to User Secrets, then exit.
if (args.Contains("--setup-auth"))
{
    AuthSetup.Run();
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Which storage the API uses: "Json" (file) or "SqlServer" (Docker container).
var dataSource = builder.Configuration["DataSource"] ?? "Json";
var authSettings = builder.Configuration.GetSection("Authentication").Get<AuthSettings>() ?? new AuthSettings();

builder.Services
    .AddControllers(options =>
    {
        // Validation error keys use the JSON names ("durationMinutes"),
        // not the C# property names ("DurationMinutes").
        options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
    })
    .AddJsonOptions(options =>
    {
        // Clients send and receive "Running" instead of 0.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// --- Data source switch: one interface, two implementations, chosen by appsettings ---
switch (dataSource)
{
    case "Json":
        // Singleton: the file lock lives inside the instance.
        builder.Services.AddSingleton<IWorkoutRepository, JsonWorkoutRepository>();
        break;

    case "SqlServer":
        var connectionString = builder.Configuration.GetConnectionString("FitTrack")
            ?? throw new InvalidOperationException(
                "DataSource is SqlServer but ConnectionStrings:FitTrack is not set. See the README (User Secrets).");
        builder.Services.AddDbContext<WorkoutDbContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddScoped<IWorkoutRepository, EfWorkoutRepository>();
        break;

    default:
        throw new InvalidOperationException($"Unknown DataSource '{dataSource}'. Use \"Json\" or \"SqlServer\".");
}

builder.Services.AddScoped<IWorkoutService, WorkoutService>();

// --- Authentication: reads stay public; writes need a JWT only when Authentication:Enabled is true ---
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Authentication"));
builder.Services.AddSingleton<ITokenService, TokenService>();

if (authSettings.Enabled)
{
    if (string.IsNullOrWhiteSpace(authSettings.Jwt.SigningKey) || string.IsNullOrWhiteSpace(authSettings.User.PasswordHash))
    {
        throw new InvalidOperationException(
            "Authentication is enabled, but the signing key or demo user is missing. " +
            "Run: dotnet run --project FitTrack.Api --launch-profile https -- --setup-auth");
    }

    var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(authSettings.Jwt.SigningKey));

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authSettings.Jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = authSettings.Jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
}

builder.Services.AddAuthorization(options =>
{
    // POST, PUT, and DELETE use this policy. With authentication off it approves everyone.
    options.AddPolicy("WorkoutWrite", policy =>
    {
        if (authSettings.Enabled)
            policy.RequireAuthenticatedUser();
        else
            policy.RequireAssertion(_ => true);
    });
});

// --- Login rate limit: 5 attempts per minute per client address, then 429 ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        var response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too many login attempts",
            Detail = "Wait a minute and try again."
        };

        await response.WriteAsJsonAsync(problem, (JsonSerializerOptions?)null, "application/problem+json", cancellationToken);
    };
});

var app = builder.Build();

// SQL Server: create or update the tables from the EF Core migration before serving requests.
if (dataSource == "SqlServer")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
    await db.Database.MigrateAsync();
    app.Logger.LogInformation("Database migrations applied");
}

// One place for unexpected errors; returns a problem-details 500.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRateLimiter();

if (authSettings.Enabled)
    app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();

// Startup lines the video shows: which data source, and whether writes need a token.
app.Logger.LogInformation("Data source: {DataSource}", dataSource);
app.Logger.LogInformation(
    "Authentication: {AuthenticationState}",
    authSettings.Enabled ? "ON (POST, PUT, and DELETE require a valid token)" : "OFF (anyone can write)");

app.Run();

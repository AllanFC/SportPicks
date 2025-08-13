using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog.Sinks.PostgreSQL;
using Serilog;
using SportPicks.API.Authorization;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Common.Interfaces;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Infrastructure.ExternalApis.Espn;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Users.Services;
using Domain.Common;
using Scalar.AspNetCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var postgresqlDb = builder.Configuration.GetConnectionString("SportPicksDb");

// Add Serilog to the application
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console() // Logs to console
    .WriteTo.PostgreSQL(
        connectionString: postgresqlDb,
        tableName: "Logs",
        needAutoCreateTable: true,
        columnOptions: new Dictionary<string, ColumnWriterBase>
        {
            { "Timestamp", new TimestampColumnWriter() },
            { "Level", new LevelColumnWriter() },
            { "Message", new RenderedMessageColumnWriter() },
            { "Exception", new ExceptionColumnWriter() },
            { "Properties", new PropertiesColumnWriter() }
        })
    .CreateLogger();

// Set Serilog as the default logger
builder.Host.UseSerilog();

// Configure options pattern
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<NflSyncSettings>(builder.Configuration.GetSection("NflSync"));

// Add services to the container
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

// Add multi-sport repositories
builder.Services.AddScoped<ISeasonRepository, SeasonRepository>();
builder.Services.AddScoped<ICompetitorRepository, CompetitorRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();

// Add NFL season services (still needed for date calculations)
builder.Services.AddScoped<INflSeasonService, NflSeasonService>();

// Add NFL data synchronization services
builder.Services.AddScoped<INflDataSyncService, NflDataSyncService>();
builder.Services.AddScoped<ISeasonSyncService, SeasonSyncService>();

// Add HttpClient for ESPN API
builder.Services.AddHttpClient<EspnApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IEspnApiClient, EspnApiClient>();

// Add DbContext with proper configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(postgresqlDb, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("SportPicks.Infrastructure");
        npgsqlOptions.EnableRetryOnFailure(3);
    });
    
    // Only enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // Require HTTPS in production
        options.SaveToken = false; // Don't save tokens in AuthenticationProperties for security
        options.MapInboundClaims = false; // Keep original claim names
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1), // Reduce clock skew for tighter security
            
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // Add custom event handlers for better logging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Logger.Warning("JWT authentication failed: {Exception}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Logger.Debug("JWT token validated for user: {UserId}", 
                    context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return Task.CompletedTask;
            }
        };
    });

// Add Authorization with custom policies
builder.Services.AddAuthorization(AuthorizationPolicies.ConfigurePolicies);

// Add controllers - use defaults (no unnecessary suppressions)
builder.Services.AddControllers();

// Configure OpenAPI with authentication
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// Apply database migrations on startup
await ApplyMigrationsAsync(app);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("SportPicks API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}
else
{
    // Use global exception handling middleware in production
    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    app.MapScalarApiReference();
}

// Security headers middleware should be early in pipeline
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    await next();
});

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserId", httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        diagnosticContext.Set("UserRole", httpContext.User.FindFirst(ClaimTypes.Role)?.Value);
    };
});

app.UseHttpsRedirection();

// Configure CORS - use localhost only (no fake domains)
var frontendUrl = builder.Configuration.GetValue<string>("Frontend:Url") ?? "http://localhost:3000";

app.UseCors(policy => policy
    .WithOrigins(frontendUrl)
    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
    .WithHeaders("Content-Type", "Authorization")
    .AllowCredentials()
    .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Logger.Information("Starting SportPicks API application");
    app.Run();
}
catch (Exception ex)
{
    Log.Logger.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Applies database migrations on startup with proper error handling
/// </summary>
static async Task ApplyMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Checking for pending database migrations...");
        
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migrations: {Migrations}", 
                pendingMigrations.Count(), string.Join(", ", pendingMigrations));
            
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }
        else
        {
            logger.LogInformation("Database is up to date - no pending migrations");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply database migrations - application will continue but may have issues");
        
        // In development, you might want to throw to prevent startup with a broken DB
        // In production, you might want to continue and handle gracefully
        if (app.Environment.IsDevelopment())
        {
            logger.LogCritical("Stopping application due to migration failure in development environment");
            throw;
        }
    }
}

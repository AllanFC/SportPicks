using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog.Sinks.PostgreSQL;
using Serilog;
using SportPicks.API.Authorization;
using Microsoft.OpenApi.Models;

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

// Add NFL data synchronization services
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<ISeasonRepository, SeasonRepository>();
builder.Services.AddScoped<INflSeasonService, NflSeasonService>();
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
        
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1), // Reduce clock skew for tighter security
            
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // Add custom event handlers for better logging
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
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

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog.Sinks.PostgreSQL;
using Serilog;

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

// Configure options pattern.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Add services to the container.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();


// Add DbContext using PostgreSQL connection string from configuration

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(postgresqlDb,
    b => b.MigrationsAssembly("SportPicks.Infrastructure")));

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;  // For dev purposes, set to true in production.
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

string frontendUrl = builder.Environment.IsDevelopment()
    ? "http://localhost:3000"
    : "http://localhost:3000"; // TODO: Change this to the production URL

app.UseCors(builder => builder
    .WithOrigins(frontendUrl) // Specify the frontend origin explicitly
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.MapControllers();

app.Run();

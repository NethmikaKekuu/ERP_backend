using CustomerService.Common.Middleware;
using CustomerService.Data;
using CustomerService.Services;
using CustomerService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ✅ FIX: Instead of ServerVersion.AutoDetect (which connects immediately and crashes
//         if MySQL isn't up), we specify the MySQL version manually.
//         MySqlServerVersion(new Version(8, 0, 36)) means MySQL 8.0.36 —
//         adjust the numbers to match your actual MySQL version if needed.
//         Common versions: 8.0.x, 5.7.x. If you're using MariaDB use MariaDbServerVersion instead.
builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new Exception("DefaultConnection is missing from appsettings.json.");

    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 36)) // ✅ Fixed version — no live connection needed at startup
    );
});

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddHttpClient<IProductProxyService, ProductProxyService>();
builder.Services.AddHttpClient<IOrderProxyService, OrderProxyService>();

var jwtKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new Exception("JWT SecretKey is missing.");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new Exception("JWT Issuer is missing.");
var jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? throw new Exception("JWT Audience is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        RoleClaimType = "role"
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:FrontendUrl"] ?? "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "InsightERP Customer Commerce API",
        Version = "v1",
        Description = "Customer-facing commerce service for ecommerce integration"
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Enter JWT token like: Bearer your_token_here",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

// ✅ FIX: Swagger was registered twice in your original code.
//         Keep it only here, outside the if-block, so it always works.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CustomerService API v1"); // ✅ Fixed path (was "v1/swagger.json")
    c.RoutePrefix = "swagger";
});

app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ✅ FIX: Wrap EnsureCreated in a try/catch so a DB connection failure at startup
//         gives you a clear warning instead of crashing the whole app.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        dbContext.Database.EnsureCreated();
        logger.LogInformation("✅ Database connection successful and schema ensured.");
    }
    catch (Exception ex)
    {
        logger.LogWarning("⚠️ Could not connect to the database at startup: {Message}", ex.Message);
        logger.LogWarning("The app will still start. Fix your DB connection and restart.");
    }
}

app.Run();
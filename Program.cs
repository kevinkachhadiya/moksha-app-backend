using MAPI.Controllers;
using MAPI.Models;
using MAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"/app/keys"))
    .SetApplicationName("MAPI")
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });

// Add Services to the Container
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<BillingServices>();

// Environment-based Database Configuration
var environment = builder.Environment.EnvironmentName;

if (builder.Environment.IsDevelopment())
{
    // Development (Local) - Use SQL Server
    var devDbConnection = builder.Configuration.GetConnectionString("DevDB")
                          ?? throw new ArgumentNullException("DevDB connection string is missing.");
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(devDbConnection));
    Console.WriteLine("[INFO] Using Development Database (SQL Server)");
}
else
{
    // Production (Render) - Use PostgreSQL
    var prodDbConnection = Environment.GetEnvironmentVariable("DATABASE_URL")
                           ?? throw new ArgumentNullException("DATABASE_URL is missing.");
    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(prodDbConnection));
    Console.WriteLine("[INFO] Using Production Database (PostgreSQL)");
}

// Authentication Configuration (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                     ?? builder.Configuration["Jwt:Issuer"]
                     ?? throw new ArgumentNullException("JWT_ISSUER is missing");

        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                       ?? builder.Configuration["Jwt:Audience"]
                       ?? throw new ArgumentNullException("JWT_AUDIENCE is missing");

        var key = Environment.GetEnvironmentVariable("JWT_KEY")
                  ?? builder.Configuration["Jwt:Key"]
                  ?? throw new ArgumentNullException("JWT_KEY is missing");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Build the Application
var app = builder.Build();

// Configure Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();

// Environment-Specific Configuration
if (!app.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Urls.Add($"http://0.0.0.0:{port}");
    Console.WriteLine($"[INFO] Running in Production on port {port}");
}

// Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AuthController}/{action=Authenticate}/{id?}");

// Run Application
app.Run();

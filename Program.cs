using MAPI.Controllers;
using MAPI.Models;
using MAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Services to the Container
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<BillingServices>();

// Load Environment Variables
builder.Configuration.AddEnvironmentVariables();
var env = builder.Environment;

// Data Protection Configuration
if (env.IsDevelopment())
{
    builder.Services.AddDataProtection()
        .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
        {
            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
            ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
        });

    var devDbConnection = builder.Configuration.GetConnectionString("DevDB")
                          ?? throw new ArgumentNullException("DevDB connection string is missing.");
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(devDbConnection));
}
else
{
    var keysDirectory = Environment.GetEnvironmentVariable("KEYS_DIRECTORY") ?? "/app/keys";
    Directory.CreateDirectory(keysDirectory);

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
        .SetApplicationName("MAPI-App")
        .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
        {
            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
            ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
        });

    var prodDbConnection = Environment.GetEnvironmentVariable("DATABASE_URL")
                           ?? throw new ArgumentNullException("DATABASE_URL is missing.");

    if (prodDbConnection.StartsWith("postgres://"))
    {
        prodDbConnection = ConvertPostgresUrlToConnectionString(prodDbConnection);
    }

    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(prodDbConnection));
}

// JWT Authentication Configuration
var jwtConfig = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtConfig["Key"] ?? throw new ArgumentNullException("Jwt:Key is missing");
var jwtIssuer = jwtConfig["Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer is missing");
var jwtAudience = jwtConfig["Audience"] ?? throw new ArgumentNullException("Jwt:Audience is missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !env.IsDevelopment();
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    RequireHeaderSymmetry = false,
    ForwardLimit = null
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Production-specific settings
if (!env.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8090";
    app.Urls.Add($"http://0.0.0.0:{port}");
    Console.WriteLine($"[INFO] Running in Production on port {port}");
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Authenticate}/{id?}");

app.Run();

// Helper Method for PostgreSQL Connection String Conversion
static string ConvertPostgresUrlToConnectionString(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');
    return $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.AbsolutePath.TrimStart('/')};SSL Mode=Require;Trust Server Certificate=true;";
}

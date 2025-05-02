using MAPI.Controllers;
using MAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// ✅ Load Environment Variables
builder.Configuration.AddEnvironmentVariables();

// ✅ Configure Database Connection
string connectionString;
if (env.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("DevDB")
                      ?? throw new ArgumentNullException("DevDB connection string is missing.");

    // Use the connection string for Npgsql (PostgreSQL)
   // builder.Services.AddDbContext<AppDbContext>(options =>
       // options.UseNpgsql(connectionString));
  
    builder.Services.AddDbContext<AppDbContext>(options =>
      options.UseSqlServer(builder.Configuration.GetConnectionString("DevDB")));


}
else
{
    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                       ?? throw new ArgumentNullException("DATABASE_URL is missing.");

    if (connectionString.StartsWith("postgres://"))
    {
        connectionString = ConvertPostgresUrlToConnectionString(connectionString);
        builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
    }
}



//builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<MAPI.Services.StockService>();
builder.Services.AddScoped<MAPI.Controllers.BillingServices>();
builder.Services.AddScoped<MAPI.Controllers.SellerBillingController>();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>();

// ✅ JWT Authentication Configuration
var jwtConfig = builder.Configuration.GetSection("Jwt");
var jwtKey =Environment.GetEnvironmentVariable("JWT_KEY") ?? jwtConfig["Key"] ?? throw new ArgumentNullException("Jwt:Key is missing");
var jwtIssuer =Environment.GetEnvironmentVariable("JWT_ISSUER") ?? jwtConfig["Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer is missing");
var jwtAudience =Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? jwtConfig["Audience"] ?? throw new ArgumentNullException("Jwt:Audience is missing");

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

// ✅ CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("RenderPolicy", policy =>
    {
        policy.WithOrigins(
            "https://moksha-app-frontend.onrender.com",
            "https://moksha-app-backend.onrender.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });

    if (env.IsDevelopment())
    {
        options.AddPolicy("DevPolicy", policy =>
        {
            policy.WithOrigins("http://localhost")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    }
});

var app = builder.Build();

// ✅ Middleware Configuration
app.UseSwagger();
app.UseSwaggerUI();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    RequireHeaderSymmetry = false,
    ForwardLimit = null
});

// ✅ Ensure CORS is Applied Before Authentication
app.UseCors(env.IsDevelopment() ? "DevPolicy" : "RenderPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ✅ Production Port Fix
if (!env.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
    builder.Configuration["Kestrel:Endpoints:Http:Url"] = $"http://0.0.0.0:{port}";
    app.Urls.Add($"http://0.0.0.0:{port}");
    Console.WriteLine($"[INFO] Running on port {port}");
}

// ✅ Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Authenticate}/{id?}");

app.Run();

// ✅ Helper: Convert Postgres URL to Connection String
static string ConvertPostgresUrlToConnectionString(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');
    return $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.AbsolutePath.TrimStart('/')};SSL Mode=Require;Trust Server Certificate=true;";
}

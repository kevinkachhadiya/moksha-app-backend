using MAPI.Controllers;
using MAPI.Models;
using MAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<BillingServices>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevDB")));
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        var key = builder.Configuration["Jwt:Key"];

        if (string.IsNullOrEmpty(issuer))
        {
            throw new ArgumentNullException("Jwt:Issuer", "Issuer configuration is missing.");
        }

        if (string.IsNullOrEmpty(audience))
        {
            throw new ArgumentNullException("Jwt:Audience", "Audience configuration is missing.");
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException("Jwt:Key", "Key configuration is missing.");
        }

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AuthController}/{action=Authenticate}/{id?}");

app.Run();

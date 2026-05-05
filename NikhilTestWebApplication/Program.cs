using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NikhilTestWebApplication.Data;
using NikhilTestWebApplication.Interfaces;
using NikhilTestWebApplication.Middleware;
using NikhilTestWebApplication.Models;
using NikhilTestWebApplication.Services;
using NikhilTestWebApplication.Validators;
using System.Text;
using NikhilTestWebApplication.Mappings;
using Hangfire;
using Hangfire.MemoryStorage;

//serilog
Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day).CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(UserProfile));
builder.Services.AddMemoryCache();
builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

//validation
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        var response = new ErrorResponse
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});

// JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(key)
            };
    });

builder.Services.AddAuthorization();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration
            .GetConnectionString("DefaultConnection")
    ));

// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// HttpClient
builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress =
        new Uri("https://localhost:7036/");
});

builder.Services.AddScoped<UserServiceClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();

app.MapControllers();

app.Run();
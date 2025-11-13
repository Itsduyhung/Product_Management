using CloudinaryDotNet;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Products_Management.Configuration;
using Products_Management.DTOs.Request;
using Products_Management.Repository;
using Products_Management.Repository.Interface;
using Products_Management.Service;
using Products_Management.Service.Interface;
using Products_Management.Services.Interfaces;
using System.Text;

namespace Products_Management
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== CONTROLLERS & SWAGGER =====
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Products Management API", Version = "v1" });

                // 🔒 Thêm cấu hình JWT Bearer cho Swagger
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Nhập token theo định dạng: Bearer {your JWT token}"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
            });


            // ===== FLUENT VALIDATION =====
            builder.Services.AddFluentValidationAutoValidation()
                            .AddFluentValidationClientsideAdapters();
            builder.Services.AddValidatorsFromAssemblyContaining<ProductRequestValidator>();

            // ===== CORS CONFIGURATION =====
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                    {
                        Console.WriteLine($"🔍 Origin: {origin}");

                        if (origin.Contains("localhost")) return true;
                        if (origin.Contains("vercel.app")) return true;
                        return false;
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            // ===== LOGGING =====
            builder.Services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            // ===== DATABASE: PostgreSQL =====
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ===== REPOSITORY + SERVICE =====
            builder.Services.AddScoped<IEntityRepository, EntityRepository>();
            builder.Services.AddScoped<IEntityService, EntityService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<PayOSService>();

            // Configure PayOS HttpClient
            builder.Services.AddHttpClient("PayOS", client =>
            {
                client.BaseAddress = new Uri("https://api-merchant.payos.vn");
            });

            // ===== CLOUDINARY =====
            var cloudName = builder.Configuration["CloudinarySettings:CloudName"];
            var apiKey = builder.Configuration["CloudinarySettings:ApiKey"];
            var apiSecret = builder.Configuration["CloudinarySettings:ApiSecret"];
            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account) { Api = { Secure = true } };
            builder.Services.AddSingleton(cloudinary);

            // ===== JWT CONFIGURATION =====
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var jwtKey = jwtSettings["Key"] ?? throw new ArgumentNullException("JWT Key is missing in configuration");
            var key = Encoding.UTF8.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // tránh trễ thời gian token
                };
            });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // ===== GLOBAL EXCEPTION & CORS HEADERS =====
            app.Use(async (context, next) =>
            {
                var origin = context.Request.Headers["Origin"].ToString();

                if (!string.IsNullOrEmpty(origin) &&
                    (origin.Contains("localhost") || origin.Contains("vercel.app")))
                {
                    context.Response.Headers["Access-Control-Allow-Origin"] = origin;
                    context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                    context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With";
                    context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
                }

                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.StatusCode = 204;
                    return;
                }

                await next();
            });

            // ===== SWAGGER =====
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Products Management API v1");
                // Serve the Swagger UI at /swagger (matches launchSettings launchUrl)
                c.RoutePrefix = "swagger";
            });

            // ===== MIDDLEWARE PIPELINE =====
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            app.UseCors("AllowReactApp");
            app.UseRouting();

            app.UseAuthentication(); // ✅ phải đặt trước Authorization
            app.UseAuthorization();

            app.MapControllers();

            // Redirect root path to Swagger UI
            app.MapGet("/", () => Results.Redirect("/swagger"));

            app.Run();
        }
    }
}
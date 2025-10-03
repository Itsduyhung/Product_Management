using FluentValidation;
using FluentValidation.AspNetCore;
using Products_Management.API;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;

namespace Products_Management
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Products Management API", Version = "v1" });
            });

            builder.Services.AddFluentValidationAutoValidation()
                            .AddFluentValidationClientsideAdapters();
            builder.Services.AddValidatorsFromAssemblyContaining<EntityRequestValidator>();

            // ===== CORS CONFIGURATION - AGGRESSIVE MODE =====
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                    {
                        // Log để debug
                        Console.WriteLine($"🔍 Origin: {origin}");

                        // Allow localhost
                        if (origin.Contains("localhost")) return true;

                        // Allow Vercel
                        if (origin.Contains("vercel.app")) return true;

                        return false;
                    })
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // PostgreSQL
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Repository + Service
            builder.Services.AddScoped<IEntityRepository, EntityRepository>();
            builder.Services.AddScoped<IEntityService, EntityService>();

            // Cloudinary
            var cloudName = builder.Configuration["CloudinarySettings:CloudName"];
            var apiKey = builder.Configuration["CloudinarySettings:ApiKey"];
            var apiSecret = builder.Configuration["CloudinarySettings:ApiSecret"];
            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account) { Api = { Secure = true } };
            builder.Services.AddSingleton(cloudinary);

            var app = builder.Build();

            // ===== MANUAL CORS HEADERS - Fallback solution =====
            app.Use(async (context, next) =>
            {
                var origin = context.Request.Headers["Origin"].ToString();

                if (!string.IsNullOrEmpty(origin) &&
                    (origin.Contains("localhost") || origin.Contains("vercel.app")))
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
                    context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
                    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                }

                // Handle preflight
                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.StatusCode = 204;
                    return;
                }

                await next();
            });

            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Products Management API v1");
                c.RoutePrefix = string.Empty;
            });

            // CORS Policy
            app.UseCors("AllowReactApp");

            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
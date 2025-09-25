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

            // Add Controllers + FluentValidation
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Products Management API", Version = "v1" });
            });

            builder.Services.AddFluentValidationAutoValidation()
                            .AddFluentValidationClientsideAdapters();
            builder.Services.AddValidatorsFromAssemblyContaining<EntityRequestValidator>();

            // React CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
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

            // Swagger - bật cả trong Production
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Products Management API v1");
                c.RoutePrefix = string.Empty; // Swagger UI ở "/"
            });

            // ⚠️ Tạm thời tắt HTTPS redirect trong Docker để tránh lỗi
            // app.UseHttpsRedirection();

            app.UseCors("AllowReactApp");
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Products_Management.API;
using CloudinaryDotNet;

namespace Products_Management
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000") // URL ReactJS
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            // ✅ Đăng ký DbContext cho PostgreSQL
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ✅ Đăng ký Repository
            builder.Services.AddScoped<IEntityRepository, EntityRepository>();
            builder.Services.AddScoped<IEntityService, EntityService>();

            // ✅ Config Cloudinary (đọc từ "CloudinarySettings")
            var cloudName = builder.Configuration["CloudinarySettings:CloudName"];
            var apiKey = builder.Configuration["CloudinarySettings:ApiKey"];
            var apiSecret = builder.Configuration["CloudinarySettings:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account)
            {
                Api = { Secure = true } // luôn dùng HTTPS
            };

            builder.Services.AddSingleton(cloudinary);

            // ✅ Đăng ký Service (không cần interface nữa)
            builder.Services.AddScoped<EntityService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowReactApp");
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
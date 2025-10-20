using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Token6
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controllers + Swagger (tiện test)
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ====== CORS: CHO PHÉP FRONTEND GỌI API + COOKIE ======
            // LƯU Ý: Khi .AllowCredentials() thì KHÔNG được dùng AllowAnyOrigin().
            // Hãy liệt kê đúng các origin FE của bạn (http/https, localhost/127.0.0.1).
            var allowedOrigins = new[]
            {
                "http://127.0.0.1:5500",  // Live Server HTTP
                "http://localhost:5500",
                "https://127.0.0.1:5500", // Live Server HTTPS (nếu bật)
                "https://localhost:5500",

                "http://localhost:5173",  // Vite HTTP
                "https://localhost:5173", // Vite HTTPS

                "http://localhost:3000",  // React/Next dev
                "https://localhost:3000"
            };

            builder.Services.AddCors(opt =>
            {
                opt.AddPolicy("FrontendOnly", p => p
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            // ====== JWT ======
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new Exception("SecretKey missing");
            var issuer = jwtSettings["Issuer"] ?? throw new Exception("Issuer missing");
            var audience = jwtSettings["Audience"] ?? throw new Exception("Audience missing");

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Swagger cho môi trường Dev
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // QUAN TRỌNG: CORS phải chạy TRƯỚC Authentication/Authorization
            app.UseCors("FrontendOnly");

            app.UseAuthentication();
            app.UseAuthorization();

            // (Tuỳ chọn) Nếu bạn muốn phục vụ file tĩnh/FE từ wwwroot
            // app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }
    }
}

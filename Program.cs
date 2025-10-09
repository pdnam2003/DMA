using LoginApi.Services;

namespace LoginApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // 👉 set WebRootPath ngay lúc khởi tạo builder
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                WebRootPath = "wwwroot"          // ép webroot đúng thư mục wwwroot trong project
                // ContentRootPath: để mặc định (thư mục project)
            });

            builder.Services.AddControllers();

            builder.Services.Configure<FileUserStore.UserStoreOptions>(
                builder.Configuration.GetSection("UserStore"));
            builder.Services.AddSingleton<IUserStore, FileUserStore>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // phục vụ file tĩnh
            app.UseDefaultFiles();   // tự tìm index.html
            app.UseStaticFiles();

            // route chẩn đoán (tạm) để xem webroot thực
            app.MapGet("/_whoami", (IWebHostEnvironment env) =>
            {
                var webRoot = env.WebRootPath ?? "(null)";
                var contentRoot = env.ContentRootPath ?? "(null)";
                var files = System.IO.Directory.Exists(webRoot)
                    ? System.IO.Directory.GetFiles(webRoot).Select(Path.GetFileName)
                    : Array.Empty<string>();
                return Results.Json(new { contentRoot, webRoot, files });
            });

            app.MapControllers();

            // fallback về index.html nếu gõ đường dẫn lạ
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}

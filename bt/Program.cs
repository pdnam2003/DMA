
using CrudApi.Services;

namespace CrudApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            const string DevCors = "DevCors";
            builder.Services.AddCors(o => o.AddPolicy(DevCors, p =>
                p.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
                 .AllowAnyHeader().AllowAnyMethod()
            ));

            builder.Services.AddControllers();

           
            builder.Services.Configure<FileProductStore.ProductStoreOptions>(
                builder.Configuration.GetSection("ProductStore"));
            builder.Services.AddSingleton<IProductStore, FileProductStore>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors(DevCors); 
            app.MapControllers();
            app.Run();
        }
    }
}

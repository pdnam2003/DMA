
using CrudApi.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CrudApi.Services
{
    public class FileProductStore : IProductStore
    {
        private readonly string _filePath;
        private readonly object _lock = new(); 

        private  static readonly JsonSerializerOptions _jsonOpts = new()
        {
          
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public class ProductStoreOptions
        {

            public string? FilePath { get; set; } = "App_Data/Product.json";

        }
         public FileProductStore(IOptions<ProductStoreOptions> opt, IWebHostEnvironment env)
        {
            var rel = string.IsNullOrWhiteSpace(opt.Value?.FilePath)
                ? "App_Data/Product.json"
                : opt.Value.FilePath!;
            _filePath = Path.Combine(env.ContentRootPath, rel);
             var dir = Path.GetDirectoryName(_filePath)!;
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if(!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        private List<Product> Load()
        {
            lock(_lock)
            {
                try
                {
                    var json = File.ReadAllText(_filePath);

                    if (string.IsNullOrWhiteSpace(json)) json = "[]";
                    
                    return JsonSerializer.Deserialize<List<Product>>(json, _jsonOpts) ?? new List<Product>();
                }
                catch
                {
                    File.WriteAllText(_filePath, "[]");
                    return new List<Product>();
                }


            }
        }

        private void Save(List<Product> products)
        {
            lock(_lock)
            {
                var json = JsonSerializer.Serialize(products, _jsonOpts);
                File.WriteAllText(_filePath, json);
            }
        }



        public Task<Product> CreateAsync(Product p)
        {
            var list = Load();
            p.Id = (list.Count()==0) ? 1: list.Max(x => x.Id) + 1;
            list.Add(p);
            Save(list);
            return Task.FromResult(p);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var list = Load();
            var removed = list.RemoveAll(p => p.Id == id)>0;
            if (removed) Save(list);
            return Task.FromResult(removed);
        }

        public Task<List<Product>> GetAllAsync()
        {
            return Task.FromResult(Load());

        }

        public Task<Product?> GetByIdAsync(int id)
        {
            var list = Load();
            return Task.FromResult(list.FirstOrDefault(p => p.Id == id));
        }

        public Task<bool> UpdateAsync(Product p)
        {
            var list =  Load();
            var idx = list.FindIndex(x => x.Id == p.Id);
            if(idx <0)  return Task.FromResult(false);
            list[idx] = p;
            Save(list);
            return Task.FromResult(true);
        }
    }
}

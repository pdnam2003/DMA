using CrudApi.Models;

namespace CrudApi.Services
{
    public interface IProductStore
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product p);
        Task<bool> UpdateAsync( Product p);
        Task<bool> DeleteAsync(int id);


    }
}

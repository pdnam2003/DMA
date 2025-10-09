using LoginApi.Models;

namespace LoginApi.Services
{
    public interface IUserStore
    {
        Task<List<User>> LoadUsersAsync();
        Task SaveUsersAsync(List<User> users);
        Task<User> FindByUsernameAsync(string username);
        Task<bool> AddUserAsync(User user);

    }
}

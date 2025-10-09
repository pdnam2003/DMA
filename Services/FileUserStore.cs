
using LoginApi.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LoginApi.Services
{
    public class FileUserStore : IUserStore
    {
        private readonly string _filePath;
        private readonly object _lock = new();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public class UserStoreOptions
        {
            public string FilePath { get; set; } = "users.json";
        }

        public FileUserStore(IOptions<UserStoreOptions> opstions, IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, opstions.Value.FilePath);
            var dir = Path.GetDirectoryName(_filePath);
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir!);
            }
            if(!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }


        }

        public Task<List<User>> LoadUsersAsync()
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_filePath);
                var users = JsonSerializer.Deserialize<List<User>>(json, _jsonOptions) ?? new List<User>();
                return Task.FromResult(users);
            }
        }

        public Task SaveUsersAsync(List<User> users)
        {
            lock (_lock)
            {
               var json = JsonSerializer.Serialize(users, _jsonOptions);
                File.WriteAllText(_filePath, json);
                return Task.CompletedTask;
            }
        }

        public Task<User> FindByUsernameAsync(string username)
        {
            lock (_lock)
            {
                var users = LoadUsersAsync().Result; // Wait for the task to complete and get the result
                var user = users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult(user);
            }
        }

        public async Task<bool> AddUserAsync(User user)
        {
            var users = await LoadUsersAsync();
            if (users.Any(u => string.Equals(u.Username, user.Username, StringComparison.OrdinalIgnoreCase))) 
            {
                return false;
            };


            users.Add(user);
            await SaveUsersAsync(users);
            return true;


        }
    }
}

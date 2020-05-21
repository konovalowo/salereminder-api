using ProductApi.Models;
using System.Threading.Tasks;

namespace ProductApi.Services
{
    public interface IUserService
    {
        public Task<User> Authenticate(string email, string password);

        public Task<User> Register(User user);

        public Task<bool> RegisterFirebaseToken(string userId, string token);
    }
}

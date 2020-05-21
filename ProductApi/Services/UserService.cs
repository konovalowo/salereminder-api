using ProductApi.Models;
using ProductApi.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Services
{
    public class UserService : IUserService
    {
        private readonly ProductContext _context;

        public UserService(ProductContext context)
        {
            _context = context;
        }

        public async Task<User> Authenticate(string email, string password)
        {
            var user = await Task.Run(() => _context.Users.ToList().SingleOrDefault(
                u => u.Email == email
                && PasswordHasher.ComparePasswords(password, u)));

            if (user == null)
            {
                return null;
            }

            return user;
        }

        public async Task<User> Register(User user)
        {
            if (_context.Users.FirstOrDefault(x => (x.Email == user.Email)) != null)
            {
                throw new ArgumentException("User already exists");
            }

            byte[] hashedPasswordSalt = PasswordHasher.HashPassword(user.Password, PasswordHasher.GenerateSalt());

            user.Password = Convert.ToBase64String(hashedPasswordSalt);
            UserProfile userProfile = new UserProfile { User = user, UserId = user.Id};

            _context.Users.Add(user);
            _context.UserProfiles.Add(userProfile);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> RegisterFirebaseToken(string userId, string token)
        {
            var userProfile = _context.UserProfiles.Single(u => u.UserId.ToString() == userId);
            if (userProfile.FirebaseToken == null)
            {
                userProfile.FirebaseToken = token;
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                userProfile.FirebaseToken = token;
                await _context.SaveChangesAsync();
                return false;
            }
        }
    }
}

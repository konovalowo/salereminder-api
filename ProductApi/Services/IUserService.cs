using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductApi.Models;

namespace ProductApi.Services
{
    public interface IUserService
    {
        public Task<User> Authenticate(User user);

        public Task<User> Register(User user);
    }
}

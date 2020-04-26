using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public List<UserProfileProduct> UserProducts { get; set; }

        public UserProfile()
        {
            UserProducts = new List<UserProfileProduct>();
        }
    }
}

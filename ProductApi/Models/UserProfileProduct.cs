using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Models
{
    public class UserProfileProduct
    {
        public int UserId { get; set; }
        public UserProfile User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}

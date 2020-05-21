using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models
{
    public class UserProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string FirebaseToken { get; set; }

        public List<UserProfileProduct> UserProducts { get; set; }

        public UserProfile()
        {
            UserProducts = new List<UserProfileProduct>();
        }
    }
}

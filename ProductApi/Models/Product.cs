using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProductApi.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Currency { get; set; }
        public double Price { get; set; }
        public bool IsOnSale { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        [JsonIgnore]
        public List<UserProfileProduct> UserProducts { get; set; }

        public Product()
        {
            UserProducts = new List<UserProfileProduct>();
        }
    }
}

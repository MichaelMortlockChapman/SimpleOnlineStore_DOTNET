using System.ComponentModel.DataAnnotations;

namespace SimpleOnlineStore_Dotnet.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; private set; }
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(400)]
        public string Description { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }

        /// <summary>
        /// Neeed as it is used by the DB enity constructor
        /// </summary>
        private Product() {}

        public Product(string name, string decription, double price, int stock)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this.Description = decription;
            this.Price = price;
            this.Stock = stock;
        }
    }

    public record class ProductDetails
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
    }
}

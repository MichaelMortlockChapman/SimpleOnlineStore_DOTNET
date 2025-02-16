namespace SimpleOnlineStore_Dotnet.Models {
    public class Order {
        public Guid Id { get; set; }

        public ICollection<Product> Products { get; set; }
        public ICollection<int> ProductQuantities { get; set; }

        public Customer Customer { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public int PostalCode { get; set; }

        public string Country { get; set; }

        public string Status { get; set; }

        public DateTime DateCreated { get; set; }

        public Order() {
            Products = new List<Product>();
            ProductQuantities = new List<int>();
        }

        public Order(ICollection<Product> products, ICollection<int> productQuantities, Customer customer, string address, string city,
            int postalCode, string country, string status) {
            this.Products = products;
            this.ProductQuantities = productQuantities;
            this.Customer = customer;
            this.Address = address;
            this.City = city;
            this.PostalCode = postalCode;
            this.Country = country;
            this.Status = status;
            this.DateCreated = DateTime.Now;
        }

        public string ToJSON() {
            return
              "{"
                    + $"Id:{Id},"
                    + $"CustomerId:{Customer.Id},"
                    + $"Address:{Address},"
                    + $"City:{City},"
                    + $"PostalCode:{PostalCode},"
                    + $"Country:{Country},"
                    + $"ProductIds:[{String.Join(",", Products.Select(p => p.Id.ToString()))}],"
                    + $"ProductQuantities:[{String.Join(",", ProductQuantities)}],"
                    + $"DateCreated:{DateCreated.ToUniversalTime()}"
            + "}";
        }
    }
}
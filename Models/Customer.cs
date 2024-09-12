using System.ComponentModel.DataAnnotations;

namespace SimpleOnlineStore_Dotnet.Models
{
    public class Customer
    {
        [Key]
        public Guid id { get; set; }

        public string name { get; set; }

        public string address { get; set; }

        public string city { get; set; }

        public int postalCode { get; set; }

        public string country { get; set; }

        public Customer() { }

        public Customer(string name, string address,
            string city, int postalCode, string country)
        {
            this.name = name;
            this.address = address;
            this.city = city;
            this.postalCode = postalCode;
            this.country = country;
        }
    }
}
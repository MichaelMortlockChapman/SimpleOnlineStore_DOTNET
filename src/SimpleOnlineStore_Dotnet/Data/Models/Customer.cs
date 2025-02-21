using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;
using System.Net;
using System.Xml.Linq;

namespace SimpleOnlineStore_Dotnet.Models {
    public class Customer {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        [ProtectedPersonalData]
        public string Address { get; set; }

        [ProtectedPersonalData]
        public string City { get; set; }

        [ProtectedPersonalData]
        public int PostalCode { get; set; }

        [ProtectedPersonalData]
        public string Country { get; set; }

        public Customer() {
            this.Name = default!;
            this.Address = default!;
            this.City = default!;
            this.PostalCode = default!;
            this.Country = default!;
            Id = Guid.NewGuid();
        }

        public Customer(string name, string address,
            string city, int postalCode, string country) : this() {
            this.Name = name;
            this.Address = address;
            this.City = city;
            this.PostalCode = postalCode;
            this.Country = country;
        }

        public string ToJSON() {
            return
              "{"
                    + $"Name:{Name},"
                    + $"Address:{Address},"
                    + $"City:{City},"
                    + $"PostalCode:{PostalCode},"
                    + $"Country:{Country},"
            + "}";
        }
    }
}
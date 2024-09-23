using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleOnlineStore_Dotnet.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public ICollection<Product> products { get; set; }

        public Customer customer { get; set; }

        public int quantity { get; set; }

        public string address { get; set; }

        public string city { get; set; }

        public int postalCode { get; set; }

        public string country { get; set; }

        public string status { get; set; }

        public Order() {
            products = new List<Product>();
        }

        public Order(ICollection<Product> products, Customer customer, int quantity, string address, string city,
            int postalCode, string country, string status)
        {
            this.products = products;
            this.customer = customer;
            this.quantity = quantity;
            this.address = address;
            this.city = city;
            this.postalCode = postalCode;
            this.country = country;
            this.status = status;
        }
    }   
}
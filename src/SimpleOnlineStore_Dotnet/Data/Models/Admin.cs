using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleOnlineStore_Dotnet.Models
{
    public class Admin
    {
        [Key]
        public Guid Id { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime creation { get; set; }

        public Admin creator { get; set; }

        public Admin() { }

        public Admin(DateTime creation, Admin creator) {
            this.creation = DateTime.UtcNow;
            this.creator = creator;
        }
    }
}

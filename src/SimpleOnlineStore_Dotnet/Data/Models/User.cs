using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleOnlineStore_Dotnet.Models
{
    public class User : IdentityUser
    {
        [ForeignKey(name: "userRoleId")]
        public Guid UserRoleId { get; set; }
    }
}

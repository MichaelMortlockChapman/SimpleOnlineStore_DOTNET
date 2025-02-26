﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleOnlineStore_Dotnet.Models {
    public class Admin {
        [Key]
        public Guid Id { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Creation { get; set; }

        public Admin? Creator { get; set; }

        public Admin() {
            Id = Guid.NewGuid();
        }

        public Admin(DateTime creation, Admin creator) : this() {
            this.Creation = DateTime.UtcNow;
            this.Creator = creator;
        }
    }
}

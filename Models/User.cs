using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChatGPTApp.Models
{
    [Table("User")]
    [Index("Username", Name = "UQ__User__536C85E4BA73E1A3", IsUnique = true)]
    [Index("Email", Name = "UQ__User__A9D10534BFC4585E", IsUnique = true)]
    public partial class User
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public string Username { get; set; } = null!;

        [StringLength(256)]
        public string Email { get; set; } = null!;

        [StringLength(256)]
        public string Password { get; set; } = null!;

        public bool InitialMessageSent { get; set; } = false; 

        [InverseProperty("User")]
        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}

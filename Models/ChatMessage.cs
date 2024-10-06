using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChatGPTApp.Models
{
    [Table("ChatMessage")]
    public partial class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string Role { get; set; } = null!;

        public string? Content { get; set; } = null!;

        [Column(TypeName = "datetime")]
        public DateTime Timestamp { get; set; }

        public int UserId { get; set; }

        public bool ShowInChatbox { get; set; } = true; 

        [ForeignKey("UserId")]
        [InverseProperty("ChatMessages")]
        public virtual User User { get; set; } = null!;
    }
}

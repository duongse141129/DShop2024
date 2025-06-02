using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Message")]
    public class MessageModel
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(300, ErrorMessage = "Content message can not null and maximum 300 characters")]
        public string ContentMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }
        public string ReceiverId { get; set; }
        public bool? IsRead { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUserModel User { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual AppUserModel Receiver { get; set; }
    }
}

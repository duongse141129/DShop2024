using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
    [Table("Contact")]
    public class ContactModel
    {

        [Key]
        public int Id { get; set; }
        public DateTime DateSent { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Subject { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        public string Message { get; set; }
        public string UserId { get; set; }
        public int Status { get; set; }

        public string RespondentId { get; set; }
		public DateTime DateRespone { get; set; }

		public string ReplyMessage { get; set; }

		[ForeignKey("UserId")]
        public virtual AppUserModel User { get; set; }

        [ForeignKey("RespondentId")]
        public virtual AppUserModel Respondent { get; set; }
    }
}

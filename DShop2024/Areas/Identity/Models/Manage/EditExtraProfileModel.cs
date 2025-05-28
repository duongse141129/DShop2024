using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Areas.Identity.Models.ManageViewModels
{
  public class EditExtraProfileModel
  {
          [Display(Name = "User Name")]
          public string UserName { get; set; }

          [Display(Name = "Email")]
          public string UserEmail { get; set; }
          [Display(Name = "Phone number")]
          [DataType(DataType.PhoneNumber)]
          public string PhoneNumber { get; set; }


          [Display(Name = "Birthday")]
          [DataType(DataType.Date)]
          public DateTime? BirthDate { get; set; }

          [Column(TypeName = "nvarchar")]
          [Display(Name = "Occupation")]
          [StringLength(100)]
          public string Occupation { get; set; }

        public bool? Gender { get; set; }



        [Column(TypeName = "nvarchar")]
        [StringLength(500)]
        public string? Avatar { get; set; }

        public string? RoleName { get; set; }

        public string LoginType { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile? AvatarUpload { get; set; }

    }
}
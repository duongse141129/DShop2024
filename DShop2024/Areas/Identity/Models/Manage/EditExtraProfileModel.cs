using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Areas.Identity.Models.ManageViewModels
{
  public class EditExtraProfileModel
  {
          [Display(Name = "Tên tài khoản")]
          public string UserName { get; set; }

          [Display(Name = "Địa chỉ email")]
          public string UserEmail { get; set; }
          [Display(Name = "Số điện thoại")]
          public string PhoneNumber { get; set; }

          [Display(Name = "Địa chỉ")]
          [StringLength(400)]
          public string HomeAdress { get; set; }


          [Display(Name = "Ngày sinh")]
          [DataType(DataType.Date)]
          public DateTime? BirthDate { get; set; }

          [Column(TypeName = "nvarchar")]
          [StringLength(100)]
          public string Occupation { get; set; }
    }
}
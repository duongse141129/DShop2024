// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace App.Areas.Identity.Models.ManageViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "The {0} field is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(100, ErrorMessage = "{0} must have at least {2} characteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New password confirmation")]
        [Compare("NewPassword", ErrorMessage = "Confirm password must match new password")]
        public string ConfirmPassword { get; set; }
    }
}

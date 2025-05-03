using DShop2024.Models;

namespace DShop2024.Areas.Admin.Models.User
{
    public class UserWithRoleViewModel
    {
        public AppUserModel User { get; set; }

        public string RoleName { get; set; }
    }
}

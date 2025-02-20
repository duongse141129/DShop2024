using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DShop2024.Repository
{
    public static class ModelStateExtend
    {
        public static void AddModelError(this ModelStateDictionary ModelState, string mgs)
        {
            ModelState.AddModelError(string.Empty, mgs);
        }
        public static void AddModelError(this ModelStateDictionary ModelState, IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Description);
            }
        }
    }
}

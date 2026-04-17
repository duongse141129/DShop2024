using DShop2024.Models.Request;
using DShop2024.Services.Geo;
using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Controllers
{
    public class BranchController : Controller
    {
        private readonly DShopContext _context;
        private readonly IGeoService _geoService;

        public BranchController(DShopContext context, IGeoService geoService)
        {
            _context = context;
            _geoService = geoService;
        }

        [HttpPost("Nearest")]
        public IActionResult Nearest([FromBody] LocationRequest request)
        {
            var branches = _context.Branches.Where(s => s.Status != 0).ToList();

            var nearest = branches
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.Address,
                    b.Latitude,
                    b.Longitude,
                    b.MapEmbed,
                    Distance = _geoService.CalculateDistance(
                        request.Latitude,
                        request.Longitude,
                        b.Latitude,
                        b.Longitude)
                })
                .OrderBy(x => x.Distance)
                .FirstOrDefault();

            if (nearest == null)
                return NotFound();

            return Json(new
            {
                nearest.Name,
                nearest.Address,
                nearest.Distance,
                MapEmbed = nearest.MapEmbed
            });
        }
    }
}

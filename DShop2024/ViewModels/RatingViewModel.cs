using DShop2024.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.ViewModels
{
    public class RatingViewModel
    {
        public RatingModel Rating { get; set; }
        public string ReplyByRole { get; set; }
    }
}

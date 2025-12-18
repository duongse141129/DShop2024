using AutoMapper;
using DShop2024.Areas.Admin.Models.Refund;
using DShop2024.Models;

namespace DShop2024.AutoMapper
{
    public class RefundMapper : Profile
    {
        public RefundMapper()
        {
            CreateMap<RefundModel, UpdateRefundRequest>().ReverseMap();
        }
    }
}

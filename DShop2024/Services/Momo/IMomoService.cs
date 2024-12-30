using DShop2024.Models;
using DShop2024.Models.Momo;

namespace DShop2024.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model); 
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}

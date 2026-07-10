using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Models.Vnpay;
using DShop2024.Services.Momo;
using DShop2024.Services.Vnpay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Controllers
{
    [Authorize(Roles = RoleName.Customer)]
    public class PaymentController : Controller
	{

		private readonly IVnPayService _vnPayService;
		private IMomoService _momoService;
		public PaymentController(IMomoService momoService, IVnPayService vnPayService)
		{
			_momoService = momoService;
			_vnPayService = vnPayService;
		}


	
		public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model)
		{
			var response = await _momoService.CreatePaymentAsync(model);
			return Redirect(response.PayUrl);
		}


		[HttpGet]
		public IActionResult PaymentCallBack()
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
			return View(response);
		}


	
		public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
		{
			var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

			return Redirect(url);
		}


	}
}

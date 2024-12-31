using DShop2024.Models;
using DShop2024.Models.Vnpay;
using DShop2024.Services.Momo;
using DShop2024.Services.Vnpay;
using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Controllers
{
	public class PaymentController : Controller
	{
		private readonly IVnPayService _vnPayService;
		private IMomoService _momoService;
		public PaymentController(IMomoService momoService, IVnPayService vnPayService)
		{
			_momoService = momoService;
			_vnPayService = vnPayService;
		}


		[HttpPost]
		public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
		{
			var response = await _momoService.CreatePaymentAsync(model);
			return Redirect(response.PayUrl);

		}

		[HttpGet]
		public IActionResult PaymentCallback()
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
			return View(response);
		}


		[HttpPost]
		public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
		{
			var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

			return Redirect(url);
		}


	}
}

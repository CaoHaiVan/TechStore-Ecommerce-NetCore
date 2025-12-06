using Doanchuyennganh.Models;
using Doanchuyennganh.Models.Vnpay;
using Doanchuyennganh.Repository;
using Doanchuyennganh.Services.Momo;
using Doanchuyennganh.Services.Vnpay;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Doanchuyennganh.Controllers
{
	public class PaymentController : Controller
	{
		private IMomoService _momoService;
		private readonly IVnPayService _vnPayService;

		public PaymentController(IMomoService momoService, IVnPayService vnPayService)
		{
			_momoService = momoService;
			_vnPayService = vnPayService;


		}
		[HttpPost]
		public async Task<IActionResult> CreatePaymentMomo(OrderInfo model)
		{
			var response = await _momoService.CreatePaymentMomo(model);
			return Redirect(response.PayUrl);
		}
		[HttpPost]
		public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
		{
			var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

			return Redirect(url);
		}


		[HttpGet]
        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        }
		


	}
}

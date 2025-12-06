using Doanchuyennganh.Areas.Admin.Repository;
using Doanchuyennganh.Models;
using Doanchuyennganh.Models.ViewModels;
using Doanchuyennganh.Repository;
using Doanchuyennganh.Services.Momo;
using Doanchuyennganh.Services.Vnpay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Doanchuyennganh.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IEmailSender _emailSender;
        private readonly IMomoService _momoService;
		private readonly IVnPayService _vnPayService;
		public CheckoutController(IEmailSender emailSender,DataContext context, IMomoService momoService, IVnPayService vnPayService)
		{
			_dataContext = context;
			_emailSender = emailSender;
            _momoService = momoService;
			_vnPayService = vnPayService;
		}
		public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> Checkout(string PaymentMethod, string PaymentId)
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if(userEmail == null)
			{
				return RedirectToAction("Login", "Account");
			}
			else
			{
				var ordercode = Guid.NewGuid().ToString();
				var orderItem = new OderModel();
				orderItem.OrderCode = ordercode;

				// Nhận shipping giá từ cookie
				var shippingPriceCookie = Request.Cookies["ShippingPrice"];
				decimal shippingPrice = 0;

				//Nhận coupon code từ cookie
				var coupon_code = Request.Cookies["CouponTitle"];

				if (shippingPriceCookie != null)
				{
					var shippingPriceJson = shippingPriceCookie;
					shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
				}
				else
				{
					shippingPrice = 0;
				}
				orderItem.ShippingCost = shippingPrice;
				orderItem.CouponCode = coupon_code;
				orderItem.UserName = userEmail;
                if (PaymentMethod != "MOMO" && PaymentMethod != "VnPay")
                {
                    orderItem.PaymentMethod = "COD";
                }
                else if (PaymentMethod == "VnPay")
                {
                    orderItem.PaymentMethod = "VnPay" + " " + PaymentId;
                }
                else if (PaymentMethod == "MOMO")
                {
                    orderItem.PaymentMethod = "Momo" + " " + PaymentId;
                }

                orderItem.Status = 1;
				orderItem.CreatedDate = DateTime.Now;
				_dataContext.Add(orderItem);
				_dataContext.SaveChanges();
				List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
				foreach(var cart in cartItems)
				{
					var orderdetails = new OderDetails();
					orderdetails.UserName = userEmail;
					orderdetails.OrderCode = ordercode;
					orderdetails.ProductId = cart.ProductId;
					orderdetails.Price = cart.Price;
					orderdetails.Quantity = cart.Quantity;

					//update product quantity
					var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
					product.Quantity -= cart.Quantity;
					product.Sold += cart.Quantity;
					_dataContext.Update(product);
					// --
					_dataContext.Add(orderdetails);
					_dataContext.SaveChanges();
				}
				HttpContext.Session.Remove("Cart");
				//send email order when success
				var receiver = userEmail;
				var subject = "ĐẶT HÀNG THÀNH CÔNG.";
                var message = $@"
				<html>
				<body style='font-family:Arial, sans-serif; background-color:#f5f5f5; padding:20px;'>
					<div style='max-width:600px; margin:auto; background:white; border-radius:10px; padding:20px;'>
						<h2 style='color:#4CAF50; text-align:center;'>ĐẶT HÀNG THÀNH CÔNG!</h2>
						<p>Xin chào <strong>{userEmail}</strong>,</p>
						<p>Cảm ơn bạn đã tin tưởng và mua sắm tại <strong>VanShopper</strong>.</p>
						<p>Đơn hàng của bạn đã được tạo thành công với mã đơn: 
							<strong style='color:#4CAF50;'>{ordercode}</strong>
						</p>
						<p>Phương thức thanh toán: <strong>{orderItem.PaymentMethod}</strong></p>
						<p>Chúng tôi sẽ sớm liên hệ và giao hàng đến bạn.</p>
						<hr style='border:none; border-top:1px solid #ddd;'/>
						<p style='font-size:14px; color:#777;'>Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ qua email 
							<a href='mailto:testmailwebsite123@gmail.com'>testmailwebsite123@gmail.com</a>.
						</p>
						<p style='text-align:center; color:#888;'>Trân trọng,<br/>Đội ngũ VanShopper</p>
					</div>
				</body>
				</html>";


                await _emailSender.SendEmailAsync(receiver, subject, message);

                TempData["success"] = "Đơn hàng đã tạo thành công, vui lòng chờ duyệt đơn hàng";
				return RedirectToAction("History", "Account");
			}
			return View();
		}
		[HttpGet]
		public async Task<IActionResult> PaymentCallBack(MomoInfoModel model)
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
			var requestQuery = HttpContext.Request.Query;


			if (requestQuery["resultCode"] != 0)//giao dịch không thành công thì lưu
			{
				var newMomoInsert = new MomoInfoModel
				{
					OrderId = requestQuery["orderId"],
					FullName = User.FindFirstValue(ClaimTypes.Email),
					Amount = decimal.Parse(requestQuery["Amount"]),
					OrderInfo = requestQuery["orderInfo"],
					DatePaid = DateTime.Now
				};
				_dataContext.Add(newMomoInsert);
				await _dataContext.SaveChangesAsync();
				var PaymentMethod = "MOMO";
				//tiến hành đặt đơn hàng khi thanh toán momo thành công
				await Checkout(requestQuery["orderId"], PaymentMethod);
			}
			else
			{
				TempData["success"] = "Giao dịch Momo không thành công.";
				return RedirectToAction("Index", "Cart");
			}

			return View(response);
		}
		// sửa thanh toán VnPay không lưu đơn hàng
        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.VnPayResponseCode == "00")
            {
                var newVnpayInsert = new VnpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    PaymentId = response.PaymentId,
                    DateCreate = DateTime.Now,
                };

                _dataContext.Add(newVnpayInsert);
                await _dataContext.SaveChangesAsync();

                // CHỈ SỬA DÒNG NÀY
                await Checkout("VnPay", response.PaymentId);

                return View(response);
            }

            TempData["success"] = "Giao dịch Vnpay không thành công.";
            return RedirectToAction("Index", "Cart");
        }


    }
}

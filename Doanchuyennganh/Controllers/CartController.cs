using Doanchuyennganh.Models;
using Doanchuyennganh.Models.ViewModels;
using Doanchuyennganh.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Doanchuyennganh.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        public CartController(DataContext _context)
        {
            _dataContext = _context;
        }
        public IActionResult Index()
        {
            // Lấy giỏ hàng từ session
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            // Nhận shipping giá từ cookie
            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;

            // Nhận coupon code từ cookie
            var couponCode = Request.Cookies["CouponTitle"];

            if (shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
            }

            // Tính tổng tiền sản phẩm
            decimal grandTotal = cartItems.Sum(x => x.Quantity * x.Price);

            // Nếu người dùng đã nhập mã giảm giá hợp lệ (couponCode != null)
            // thì giảm 30.000đ vào tổng
            if (!string.IsNullOrEmpty(couponCode))
            {
                // Giảm tối đa không để âm
                grandTotal = Math.Max(0, grandTotal - 30000);
            }

            // Truyền dữ liệu ra View
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = grandTotal,
                ShippingCost = shippingPrice,
                CouponCode = couponCode
            };

            return View(cartVM);
        }

        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }
        public async Task<IActionResult> Add(long Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();

            if (cartItems == null)
            {
                cart.Add(new CartItemModel(product));
            }
            else
            {
                cartItems.Quantity += 1;
            }
            HttpContext.Session.SetJson("Cart", cart);


            TempData["success"] = "Thêm sản phẩm vào giỏ hàng thành công";
            return Redirect(Request.Headers["Referer"].ToString());
        }
        public async Task<IActionResult> Decrease(long Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();

            if (cartItem.Quantity > 1)
            {
                --cartItem.Quantity;
            }
            else
            {
                cart.RemoveAll(p => p.ProductId == Id);
            }
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
			TempData["success"] = "Giảm số lượng mặt hàng trong giỏ hàng thành công";
			return RedirectToAction("Index");
        }
		public async Task<IActionResult> Increase(long Id)
		{
            ProductModel product = await _dataContext.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();
			
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();

			if (cartItem.Quantity >= 1 && product.Quantity > cartItem.Quantity)
			{
				++cartItem.Quantity;
                TempData["success"] = "Thêm sản phẩm vào giỏ hàng thành công!";

			}
			else
			{
                cartItem.Quantity = product.Quantity;
                TempData["success"] = "Đã thêm số lượng sản phẩm tối đa vào giỏ hàng thành công!";
			}
			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cart);
			}
			TempData["success"] = "Tăng số lượng sản phẩm vào giỏ hàng thành công";
			return RedirectToAction("Index");
		}
        public async Task<IActionResult> Remove(long Id)
        {
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

            cart.RemoveAll(p => p.ProductId == Id);
            if(cart.Count == 0)
            {
				HttpContext.Session.Remove("Cart");
			}
            else
            {
				HttpContext.Session.SetJson("Cart", cart);
			}
			TempData["success"] = "Xóa sản phẩm khỏi giỏ hàng thành công";
			return RedirectToAction("Index");
		}
        public async Task<IActionResult> Clear()
        {
			HttpContext.Session.Remove("Cart");
			TempData["success"] = "Xóa tất cả Mục trong giỏ hàng Thành công";
			return RedirectToAction("Index");
		}
		// tính phí shipping
		[HttpPost]
		[Route("Cart/GetShipping")]
		public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string quan, string tinh, string phuong)
		{

			var existingShipping = await _dataContext.Shippings
				.FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

			decimal shippingPrice = 0; // Set mặc định giá tiền

			if (existingShipping != null)
			{
				shippingPrice = existingShipping.Price;
			}
			else
			{
				//Set mặc định giá tiền nếu ko tìm thấy
				shippingPrice = 50000;
			}
			var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);
			try
			{
				var cookieOptions = new CookieOptions
				{
					HttpOnly = true,
					Expires = DateTimeOffset.UtcNow.AddMinutes(30),
					Secure = true // using HTTPS
				};

				Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
			}
			catch (Exception ex)
			{
				//
				Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
			}
			return Json(new { shippingPrice });
		}
		[HttpGet]
		[Route("Cart/DeleteShipping")]
		public IActionResult RemoveShippingCookie()
		{
			Response.Cookies.Delete("ShippingPrice");
			return RedirectToAction("Index","Cart");
		}
		// Hàm GetCoupon Code
		[HttpPost]
		[Route("Cart/GetCoupon")]
        public async Task<IActionResult> GetCoupon(CouponModel couponModel, string coupon_value)
        {
            // Tìm mã giảm giá trong database
            var validCoupon = await _dataContext.Coupons
                .FirstOrDefaultAsync(x => x.Name == coupon_value);

            if (validCoupon == null)
            {
                return Ok(new { success = false, message = "Mã giảm giá không tồn tại." });
            }

            // Kiểm tra hạn sử dụng
            if (validCoupon.DateExpired < DateTime.Now)
            {
                return Ok(new { success = false, message = "Mã giảm giá đã hết hạn." });
            }

            try
            {
                // Nếu hợp lệ -> lưu vào cookie (để Index đọc được)
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };

                Response.Cookies.Append("CouponTitle", validCoupon.Name, cookieOptions);
                return Ok(new { success = true, message = "Áp dụng mã giảm giá thành công! (Giảm 30.000đ)" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding apply coupon cookie: {ex.Message}");
                return Ok(new { success = false, message = "Áp dụng mã giảm giá không thành công." });
            }
        }

    }
}

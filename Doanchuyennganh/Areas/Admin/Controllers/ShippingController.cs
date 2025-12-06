using Doanchuyennganh.Models;
using Doanchuyennganh.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Doanchuyennganh.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Shipping")]
	//[Authorize(Roles = "Publisher, Author, Admin")]
	public class ShippingController : Controller
	{
        private readonly DataContext _dataContext;
        public ShippingController(DataContext context)
        {
            _dataContext = context;
        }
		[Route("Index")]
        public async Task<IActionResult> Index()
		{
            var shippingList = await _dataContext.Shippings.ToListAsync();
            ViewBag.Shippings = shippingList;
			return View();
		}
        [HttpPost]
        [Route("StoreShipping")]
        public async Task<IActionResult> StoreShipping(ShippingModel shippingModel, string phuong, string quan, string tinh, decimal price)
        {

            shippingModel.City = tinh;
            shippingModel.District = quan;
            shippingModel.Ward = phuong;
            shippingModel.Price = price;

            try
            {

                var existingShipping = await _dataContext.Shippings
                    .AnyAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

                if (existingShipping)
                {
                    return Ok(new { duplicate = true, message = "Dữ liệu trùng lặp." });
                }
                _dataContext.Shippings.Add(shippingModel);
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm phí vận chuyển thành công" });
            }
            catch (Exception)
            {

                return StatusCode(500, "Đã xảy ra lỗi khi thêm phí vận chuyển.");
            }
        }
        public async Task<IActionResult> Delete(int Id)
        {
            ShippingModel shipping = await _dataContext.Shippings.FindAsync(Id);

            _dataContext.Shippings.Remove(shipping);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Phí vận chuyển đã xóa thành công";
            return RedirectToAction("Index","Shipping");
        }
    }
}

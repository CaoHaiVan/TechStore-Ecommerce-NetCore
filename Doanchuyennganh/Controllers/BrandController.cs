
using Doanchuyennganh.Models;
using Doanchuyennganh.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Doanchuyennganh.Controllers
{
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;

        public BrandController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index(string Slug = "", string sort_by = "", string startprice = "", string endprice = "")
        {
            // Tìm thương hiệu theo Slug
            BrandModel brand = _dataContext.Brands.Where(b => b.Slug == Slug).FirstOrDefault();

            if (brand == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Slug = Slug;

            // Lấy tất cả sản phẩm thuộc brand đó
            IQueryable<ProductModel> productsByBrand = _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.BrandId == brand.Id);

            var count = await productsByBrand.CountAsync();
            if (count > 0)
            {
                // ✅ Sắp xếp sản phẩm
                if (sort_by == "price_increase")
                {
                    productsByBrand = productsByBrand.OrderBy(p => p.Price);
                }
                else if (sort_by == "price_decrease")
                {
                    productsByBrand = productsByBrand.OrderByDescending(p => p.Price);
                }
                else if (sort_by == "price_newest")
                {
                    productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
                }
                else if (sort_by == "price_oldest")
                {
                    productsByBrand = productsByBrand.OrderBy(p => p.Id);
                }

                // ✅ Lọc theo giá
                else if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
                {
                    decimal startValue;
                    decimal endValue;

                    if (decimal.TryParse(startprice, out startValue) && decimal.TryParse(endprice, out endValue))
                    {
                        productsByBrand = productsByBrand.Where(p => p.Price >= startValue && p.Price <= endValue);
                    }
                    else
                    {
                        productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
                    }
                }
                else
                {
                    productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
                }
            }

            // Trả kết quả về view
            ViewBag.BrandName = brand.Name;
            return View(await productsByBrand.ToListAsync());
        }
    }
}

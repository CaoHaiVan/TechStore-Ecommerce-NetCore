using Doanchuyennganh.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Doanchuyennganh.Repository
{
    public class DataContext : IdentityDbContext<AppUserModel>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) 
        { 
            
        }
        public DbSet<BrandModel> Brands { get; set; }
		public DbSet<SliderModel> Sliders { get; set; }

		public DbSet<ProductModel> Products { get; set; }
		public DbSet<RatingModel> Ratings { get; set; }
		public DbSet<CategoryModel> Categories { get; set; }
		public DbSet<OderModel> Orders { get; set; }
		public DbSet<OderDetails> OrderDetails { get; set; }
        public DbSet<ContactModel> Contact { get; set; }
		public DbSet<WishlistModel> Wishlists { get; set; }
		public DbSet<CompareModel> Compares { get; set; }

        public DbSet<ProductQuantityModel> ProductQuantities { get; set; }
		public DbSet<ShippingModel> Shippings { get; set; }
		public DbSet<CouponModel> Coupons { get; set; }
        public DbSet<StatisticalModel> Statisticals { get; set; }
        public DbSet<MomoInfoModel> MomoInfo { get; set; }
        public DbSet<VnpayModel> VnpayInfo{ get; set; }




    }
}

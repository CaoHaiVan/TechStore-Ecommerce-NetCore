using Doanchuyennganh.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Doanchuyennganh.Models
{
    public class ProductModel
    {
        [Key]
        public long Id { get; set; }
        [Required( ErrorMessage = "Yêu cầu nhập tên Sản Phẩm")]
        public string Name { get; set; }
       
        public string Slug { get; set; }
        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập mô tả Sản Phẩm")]
        public string Description { get; set; }
        [Required( ErrorMessage = "Yêu cầu nhập giá Sản Phẩm")]
        [Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public  decimal Price { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập giá vốn sản phẩm")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CapitalPrice { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage ="Chọn một thương hiệu")]
        public int BrandId { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Chọn một danh mục")]
        public int CategoryId { get; set; }
        public CategoryModel Category { get; set; }
        public BrandModel Brand { get; set; }
        public string Image { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }


        public RatingModel Ratings { get; set; }
        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }

    }
}

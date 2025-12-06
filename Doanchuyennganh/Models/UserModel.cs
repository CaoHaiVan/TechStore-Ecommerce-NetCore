using System.ComponentModel.DataAnnotations;

namespace Doanchuyennganh.Models
{
	public class UserModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage ="Username không được để trống")]
		public string Username { get; set; }
		[Required(ErrorMessage = "Email không được để trống"), EmailAddress]
		public string Email { get; set; }
		[DataType(DataType.Password), Required(ErrorMessage = "Password không được để trống")]
		public string Password { get; set; }

	}
}

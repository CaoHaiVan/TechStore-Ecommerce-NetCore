using Doanchuyennganh.Areas.Admin.Repository;
using Doanchuyennganh.Models;
using Doanchuyennganh.Models.ViewModels;
using Doanchuyennganh.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Doanchuyennganh.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly DataContext _dataContext;
        public AccountController(IEmailSender emailSender, 
            SignInManager<AppUserModel> signInManager, 
            UserManager<AppUserModel> userManage,
            DataContext context)
        {
            _dataContext = context;
            _signInManager = signInManager;
			_userManage = userManage;
            _emailSender = emailSender;

        }
		public IActionResult Login(string returnUrl) 
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl});
        }

		public async Task<IActionResult> UpdateAccount()
		{
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				// User is not logged in, redirect to login
				return RedirectToAction("Login", "Account"); // Replace "Account" with your controller name
			}
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			//get user by email
			var user = await _userManage.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if(user == null)
			{
				return NotFound();
			}

			return View(user);
		}
		[HttpPost]
		public async Task<IActionResult> UpdateInfoAccount(AppUserModel user)
		{
			
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			//var userEmail = User.FindFirstValue(ClaimTypes.Email);
			//get user by email
			var userById = await _userManage.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if (userById == null)
			{
				return NotFound();
			}
			else
			{
				// Hash the new password
				var passwordHasher = new PasswordHasher<AppUserModel>();
				var passwordHash = passwordHasher.HashPassword(userById, user.PasswordHash);
				userById.PasswordHash = passwordHash;
				
				_dataContext.Update(userById);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cập nhật thông tin tài khoản thành công";
				
			}

			return RedirectToAction("UpdateAccount", "Account");
		}

		public async Task<IActionResult> NewPass(AppUserModel user, string token)
		{
			var checkuser = await _userManage.Users
				.Where(u => u.Email == user.Email)
				.Where(u => u.Token == user.Token).FirstOrDefaultAsync();

			if (checkuser != null)
			{
				ViewBag.Email = checkuser.Email;
				ViewBag.Token = token;
			}
			else
			{
				TempData["error"] = "Không tìm thấy email hoặc mã thông báo không đúng";
				return RedirectToAction("ForgetPass", "Account");
			}
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
		{
			var checkuser = await _userManage.Users
				.Where(u => u.Email == user.Email)
				.Where(u => u.Token == user.Token).FirstOrDefaultAsync();

			if (checkuser != null)
			{
				//update user with new password and token
				string newtoken = Guid.NewGuid().ToString();
				// Hash the new password
				var passwordHasher = new PasswordHasher<AppUserModel>();
				var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

				checkuser.PasswordHash = passwordHash;
				checkuser.Token = newtoken;

				await _userManage.UpdateAsync(checkuser);
				TempData["success"] = "Đã cập nhật mật khẩu thành công.";
				return RedirectToAction("Login", "Account");
			}
			else
			{
				TempData["error"] = "Không tìm thấy email hoặc mã thông báo không đúng";
				return RedirectToAction("ForgetPass", "Account");
			}
			return View();
		}

		[HttpPost]
        [HttpPost]
        public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
        {
            // Tìm user theo email
            var checkMail = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                TempData["error"] = "Email không tồn tại trong hệ thống.";
                return RedirectToAction("ForgetPass", "Account");
            }

            // Tạo token reset password
            string token = Guid.NewGuid().ToString();
            checkMail.Token = token;

            _dataContext.Update(checkMail);
            await _dataContext.SaveChangesAsync();

            // Link reset mật khẩu
            string resetLink = $"{Request.Scheme}://{Request.Host}/Account/NewPass?email={checkMail.Email}&token={token}";

            // Gửi mail
            var receiver = checkMail.Email;
            var subject = $"Yêu cầu đặt lại mật khẩu cho tài khoản {checkMail.Email}";

            // Giao diện HTML đẹp + chuyên nghiệp
            var message = $@"
    <div style='font-family: Arial, sans-serif; padding: 20px; background: #f7f7f7;'>
        <div style='max-width: 500px; margin: auto; background: white; border-radius: 10px; padding: 20px;
                     box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
            
            <h2 style='text-align:center; color:#333;'>🔒 Đặt lại mật khẩu</h2>

            <p>Xin chào <b>{checkMail.Email}</b>,</p>

            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. 
            Nhấp vào nút bên dưới để tạo mật khẩu mới:</p>

            <div style='text-align:center; margin: 25px 0;'>
                <a href='{resetLink}' 
                   style='background:#007bff; color:white; padding:12px 20px; 
                          text-decoration:none; border-radius:5px; font-size:16px;'>
                    Đặt lại mật khẩu
                </a>
            </div>

            <p style='font-size:14px; color:#555;'>
                Nếu bạn không yêu cầu thay đổi, hãy bỏ qua email này. 
                Liên kết sẽ hết hạn sau một thời gian để đảm bảo an toàn.
            </p>

            <hr style='margin: 25px 0;' />

            <p style='text-align:center; color:#888; font-size:12px;'>
                Email được gửi từ hệ thống VanShopper.<br/>
                Vui lòng không trả lời email này.
            </p>
        </div>
    </div>";

            await _emailSender.SendEmailAsync(receiver, subject, message);

            TempData["success"] = "Một email đã được gửi đến địa chỉ của bạn kèm hướng dẫn đặt lại mật khẩu.";
            return RedirectToAction("ForgetPass", "Account");
        }


        public async Task<IActionResult> ForgetPass(string returnUrl)
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if(ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
                if(result.Succeeded)
                {
                    return Redirect(loginVM.ReturnUrl ?? "/");
                }
                ModelState.AddModelError("", "Username hoặc Password bị sai");
            }
            return View(loginVM);
        }
		public IActionResult Create()
		{
			return View();
		}

        public async Task<IActionResult> History()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Lấy toàn bộ đơn hàng của user
            var orders = await _dataContext.Orders
                .Where(o => o.UserName == userEmail)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            // Lấy chi tiết từng đơn (có Product)
            var orderDetails = await _dataContext.OrderDetails
                .Include(d => d.Product)
                .Where(d => d.UserName == userEmail)
                .ToListAsync();

            ViewBag.UserEmail = userEmail;
            ViewBag.OrderDetails = orderDetails;

            return View(orders);
        }


        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            try
            {
                var order = await _dataContext.Orders
                    .FirstOrDefaultAsync(o => o.OrderCode == ordercode);

                if (order == null)
                    return NotFound();

                // 🔒 Chỉ cho hủy khi đơn hàng còn mới
                if (order.Status == 1)
                {
                    order.Status = 3; // đã hủy
                    _dataContext.Update(order);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Đã hủy đơn hàng thành công.";
                }
                else if (order.Status == 2)
                {
                    TempData["error"] = "Đơn hàng đã được xử lý, không thể hủy.";
                }
                else
                {
                    TempData["error"] = "Đơn hàng đã bị hủy trước đó.";
                }
            }
            catch
            {
                return BadRequest("An error occurred while canceling the order.");
            }

            return RedirectToAction("History", "Account");
        }


        [HttpPost]
		public async Task<IActionResult> Create(UserModel user)
        {
            if (ModelState.IsValid)
            {
                AppUserModel newUser = new AppUserModel { UserName = user.Username, Email = user.Email };
                IdentityResult result = await _userManage.CreateAsync(newUser, user.Password); 
                if(result.Succeeded)
                {
                    TempData["success"] = "Tạo user thành công.";
                    return Redirect("/account/login");
                }
                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);

                }
            }
            return View(user);
        }
        public async Task<IActionResult> Logout (string returnUrl ="/")
        {
			await HttpContext.SignOutAsync();
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
		public async Task LoginByGoogle()
		{
			// Use Google authentication scheme for challenge
			await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
				new AuthenticationProperties
				{
					RedirectUri = Url.Action("GoogleResponse")
				});
		}
		public async Task<IActionResult> GoogleResponse()
		{
			// Authenticate using Google scheme
			var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

			if (!result.Succeeded)
			{
				//Nếu xác thực ko thành công quay về trang Login
				return RedirectToAction("Login");
			}

			var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
			{
				claim.Issuer,
				claim.OriginalIssuer,
				claim.Type,
				claim.Value
			});
			var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			//var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
			string emailName = email.Split('@')[0];
			//return Json(claims);
			// Check user có tồn tại không
			var existingUser = await _userManage.FindByEmailAsync(email);

			if (existingUser == null)
			{
				//nếu user ko tồn tại trong db thì tạo user mới với password hashed mặc định 1-9
				var passwordHasher = new PasswordHasher<AppUserModel>();
				var hashedPassword = passwordHasher.HashPassword(null, "123456789");
				//username thay khoảng cách bằng dấu "-" và chữ thường hết
				var newUser = new AppUserModel { UserName = emailName, Email = email };
				newUser.PasswordHash = hashedPassword; // Set the hashed password cho user

				var createUserResult = await _userManage.CreateAsync(newUser);
				if (!createUserResult.Succeeded)
				{
					TempData["error"] = "Đăng ký tài khoản thất bại. Vui lòng thử lại sau.";
					return RedirectToAction("Login", "Account"); // Trả về trang đăng ký nếu fail

				}
				else
				{
					// Nếu user tạo user thành công thì đăng nhập luôn 
					await _signInManager.SignInAsync(newUser, isPersistent: false);
					TempData["success"] = "Đăng ký tài khoản thành công.";
					return RedirectToAction("Index", "Home");
				}

			}
			else
			{
				//Còn user đã tồn tại thì đăng nhập luôn với existingUser
				await _signInManager.SignInAsync(existingUser, isPersistent: false);
			}

			return RedirectToAction("Login");
		}

		}
}

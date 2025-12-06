using Doanchuyennganh.Libraries;
using Doanchuyennganh.Models.Vnpay;

namespace Doanchuyennganh.Services.Vnpay
{
	public class VnPayService :IVnPayService
	{
		private readonly IConfiguration _configuration;
		public VnPayService(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
		{
			var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
			var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
			var tick = DateTime.Now.Ticks.ToString();
			var pay = new VnPayLibrary();
			var urlCallBack = ResolveReturnUrl(context, _configuration["Vnpay:PaymentBackReturnUrl"] ?? string.Empty);

			pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
			pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
			pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
			pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
			pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
			pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
			pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
			pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
			pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
			pay.AddRequestData("vnp_OrderType", model.OrderType);
			pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
			pay.AddRequestData("vnp_TxnRef", tick);

			var paymentUrl =
				pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

			return paymentUrl;
		}
		public PaymentResponseModel PaymentExecute(IQueryCollection collections)
		{
			var pay = new VnPayLibrary();
			var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

			return response;
		}


		private static string ResolveReturnUrl(HttpContext context, string configuredUrl)
		{
			static bool IsLocalHost(Uri uri)
			{
				if (!uri.IsAbsoluteUri) return false;
				return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
					   || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
			}

			static string BuildFromContext(HttpContext httpContext, string path)
			{
				var scheme = httpContext.Request.Headers["X-Forwarded-Proto"].FirstOrDefault()
							 ?? httpContext.Request.Scheme;
				var host = httpContext.Request.Headers["X-Forwarded-Host"].FirstOrDefault()
						   ?? httpContext.Request.Host.Value;
				path = string.IsNullOrWhiteSpace(path) ? "/Checkout/PaymentCallbackVnpay" : path;
				if (!path.StartsWith("/")) path = "/" + path.TrimStart('/');

				return $"{scheme}://{host}{path}";
			}

			if (!string.IsNullOrWhiteSpace(configuredUrl) &&
				Uri.TryCreate(configuredUrl, UriKind.Absolute, out var absoluteUri))
			{
				return IsLocalHost(absoluteUri)
					? BuildFromContext(context, absoluteUri.AbsolutePath)
					: absoluteUri.ToString();
			}

			return BuildFromContext(context, configuredUrl);
		}
	}
}

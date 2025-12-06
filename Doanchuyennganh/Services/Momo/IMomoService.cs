using Doanchuyennganh.Models.Momo;
using Doanchuyennganh.Models;

namespace Doanchuyennganh.Services.Momo
{
    public interface IMomoService
    {
		Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfo model);
		MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
	}
}

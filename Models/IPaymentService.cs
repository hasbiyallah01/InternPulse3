namespace InternPulse3.Models
{
    public interface IPayStackService
    {
        Task<PaymentResponse> GetByReferenceAsync(string reference);
        Task<PaymentUrlResponse> InitializePayment(PaymentRequest paymentRequest);
    }
}

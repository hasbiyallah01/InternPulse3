namespace InternPulse3.Dtos
{
    public class PaymentRequest
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public decimal Amount { get; set; }
    }

    public class PaystackSettings
    {
        public string SecretKey { get; set; }
        public string BaseUrl { get; set; }
        public string CallBackUrl { get; set; }
    }
}

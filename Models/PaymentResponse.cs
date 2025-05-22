namespace InternPulse3.Models
{
    public class PaymentResponse
    {
        public string id { get; set; }
        public string customer_name { get; set; }
        public string customer_email { get; set; }
        public decimal amount { get; set; }
        public string status { get; set; }
    }

    public class PaymentUrlResponse
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string AuthorizationUrl { get; set; }
    }
}

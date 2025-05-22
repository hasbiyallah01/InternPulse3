using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using InternPulse3.Context;
using InternPulse3.Dtos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;


namespace InternPulse3.Models
{
    public class PaystackService : IPayStackService
    {
        private readonly HttpClient _httpClient;
        private readonly PaystackSettings _paystackSettings;
        private readonly AppDbContext _context;

        public PaystackService(HttpClient httpClient, IOptions<PaystackSettings> paystackSettings, AppDbContext context)
        {
            _httpClient = httpClient;
            _paystackSettings = paystackSettings.Value;

            _httpClient.BaseAddress = new Uri(_paystackSettings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _paystackSettings.SecretKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _context = context;
        }

        public async Task<PaymentUrlResponse> InitializePayment(PaymentRequest paymentRequest)
        {
            var reference = Guid.NewGuid().ToString();

            var requestData = new
            {
                email = paymentRequest.CustomerEmail,
                callback_url = _paystackSettings.CallBackUrl,
                amount = (int)(paymentRequest.Amount * 100),
                reference = reference,
                name = paymentRequest.CustomerName,
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/transaction/initialize", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to initialize payment: {response.ReasonPhrase}");
            }

            var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
            string authorizationUrl = responseData.data.authorization_url;
            var payment = new Payment
            {
                Id = reference,
                CustomerEmail = paymentRequest.CustomerEmail,
                CustomerName = paymentRequest.CustomerName,
                AuthorizationUrl = authorizationUrl,
                Amount = (int)(paymentRequest.Amount * 100),
                Status = "Pending",
            };
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            var paymentresponse = new PaymentUrlResponse
            {
                Id = reference,
                CustomerEmail = paymentRequest.CustomerEmail,
                CustomerName = paymentRequest.CustomerName,
                Amount = paymentRequest.Amount,
                Status = "completed",
                AuthorizationUrl = authorizationUrl
            };

            return paymentresponse;
        }

        public async Task<PaymentResponse?> GetByReferenceAsync(string reference)
        {
            var response = await _httpClient.GetAsync($"/transaction/verify/{reference}");

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);

            bool apiStatus = Convert.ToBoolean(responseData.status);
            if (!apiStatus)
                return null;

            string transactionStatus = responseData.data.status;
            string status = transactionStatus == "success" ? "completed" : "failed";

            string email = responseData.data.customer.email;
            decimal amount = responseData.data.amount / 100m;

            var payment = await _context.Payments.FindAsync(reference);
            if (payment == null)
                return null;

            payment.Status = status;
            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                id = $"PAY-{reference.Substring(0, 3).ToUpper()}",
                customer_name = payment.CustomerName,
                customer_email = email,
                amount = amount,
                status = status
            };
        }
    }
}

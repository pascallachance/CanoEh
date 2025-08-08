namespace Domain.Models.Responses
{
    public class DeletePaymentMethodResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid PaymentMethodID { get; set; }
    }
}
namespace Domain.Models.Responses
{
    public class DeleteTaxRateResponse
    {
        public Guid ID { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
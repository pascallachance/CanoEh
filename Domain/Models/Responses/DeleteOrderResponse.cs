namespace Domain.Models.Responses
{
    public class DeleteOrderResponse
    {
        public Guid ID { get; set; }
        public int OrderNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
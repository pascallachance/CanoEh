namespace Domain.Models.Responses
{
    public class DeleteItemVariantResponse
    {
        public Guid ItemId { get; set; }
        public Guid VariantId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}
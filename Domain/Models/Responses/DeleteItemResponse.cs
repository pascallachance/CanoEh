namespace Domain.Models.Responses
{
    public class DeleteItemResponse
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}
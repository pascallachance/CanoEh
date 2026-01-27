namespace Domain.Models.Responses
{
    public class DeleteProductNodeResponse
    {
        public Guid Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

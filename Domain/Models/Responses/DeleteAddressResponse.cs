namespace Domain.Models.Responses
{
    public class DeleteAddressResponse
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = "Address successfully deleted.";
    }
}
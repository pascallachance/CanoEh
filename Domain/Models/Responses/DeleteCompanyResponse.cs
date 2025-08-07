namespace Domain.Models.Responses
{
    public class DeleteCompanyResponse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public DateTime DeletedAt { get; set; }
        public string Message { get; set; } = "Company deleted successfully.";
    }
}
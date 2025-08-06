namespace Domain.Models.Responses
{
    public class UpdateCompanyResponse
    {
        public Guid Id { get; set; }
        public Guid OwnerID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Logo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
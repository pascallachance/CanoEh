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
        
        // New fields for enhanced company information
        public string? CountryOfCitizenship { get; set; }
        public string? FullBirthName { get; set; }
        public string? CountryOfBirth { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? IdentityDocumentType { get; set; }
        public string? IdentityDocument { get; set; }
        public string? BankDocument { get; set; }
        public string? FacturationDocument { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyType { get; set; }
        public required string Email { get; set; }
        public string? WebSite { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Address3 { get; set; }
        public string? City { get; set; }
        public string? ProvinceState { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
    }
}
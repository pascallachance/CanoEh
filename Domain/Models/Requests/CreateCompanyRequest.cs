using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreateCompanyRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Logo { get; set; }
        
        // New fields for enhanced company information
        public string? CountryOfCitizenship { get; set; }
        public string? FullBirthName { get; set; }
        public string? CountryOfBirth { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? IdentityDocumentType { get; set; } // passport, Driver Licence or government delivered document
        public string? IdentityDocument { get; set; }
        public string? BankDocument { get; set; } // bank statement or credit card
        public string? FacturationDocument { get; set; } // Facturable credit card or debit card
        public string? CompanyPhone { get; set; }
        public string? CompanyType { get; set; } // public company, listed company, private company, charity organization, particular
        public required string Email { get; set; }
        public string? WebSite { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Address3 { get; set; }
        public string? City { get; set; }
        public string? ProvinceState { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }

        public Result Validate()
        {
            if (this == null)
            {
                return Result.Failure("Company data is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(Name))
            {
                return Result.Failure("Name is required.", StatusCodes.Status400BadRequest);
            }
            if (Name.Length > 255)
            {
                return Result.Failure("Name must be 255 characters or less.", StatusCodes.Status400BadRequest);
            }
            
            // Validate Email
            if (string.IsNullOrWhiteSpace(Email))
            {
                return Result.Failure("Email is required.", StatusCodes.Status400BadRequest);
            }
            if (!ValidationHelper.IsValidEmail(Email))
            {
                return Result.Failure("Email is not valid.", StatusCodes.Status400BadRequest);
            }
            
            // Validate IdentityDocumentType
            if (!string.IsNullOrEmpty(IdentityDocumentType))
            {
                var validIdentityTypes = new[] { "passport", "Driver Licence", "government delivered document" };
                if (!validIdentityTypes.Contains(IdentityDocumentType))
                {
                    return Result.Failure("IdentityDocumentType must be 'passport', 'Driver Licence', or 'government delivered document'.", StatusCodes.Status400BadRequest);
                }
            }
            
            // Validate CompanyType
            if (!string.IsNullOrEmpty(CompanyType))
            {
                var validCompanyTypes = new[] { "public company", "listed company", "private company", "charity organization", "particular" };
                if (!validCompanyTypes.Contains(CompanyType))
                {
                    return Result.Failure("CompanyType must be 'public company', 'listed company', 'private company', 'charity organization', or 'particular'.", StatusCodes.Status400BadRequest);
                }
            }
            
            return Result.Success();
        }
    }
}
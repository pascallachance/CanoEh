using Domain.Models.Responses;
using Infrastructure.Data;

namespace Domain.Models.Converters
{
    public static class CompanyConverters
    {
        public static CreateCompanyResponse ConvertToCreateCompanyResponse(this Company company)
        {
            return new CreateCompanyResponse
            {
                Id = company.Id,
                OwnerID = company.OwnerID,
                Name = company.Name,
                Description = company.Description,
                Logo = company.Logo,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt,
                CountryOfCitizenship = company.CountryOfCitizenship,
                FullBirthName = company.FullBirthName,
                CountryOfBirth = company.CountryOfBirth,
                BirthDate = company.BirthDate,
                IdentityDocumentType = company.IdentityDocumentType,
                IdentityDocument = company.IdentityDocument,
                BankDocument = company.BankDocument,
                FacturationDocument = company.FacturationDocument,
                CompanyPhone = company.CompanyPhone,
                CompanyType = company.CompanyType,
                Email = company.Email,
                WebSite = company.WebSite,
                Address1 = company.Address1,
                Address2 = company.Address2,
                Address3 = company.Address3,
                City = company.City,
                ProvinceState = company.ProvinceState,
                Country = company.Country,
                PostalCode = company.PostalCode
            };
        }

        public static GetCompanyResponse ConvertToGetCompanyResponse(this Company company)
        {
            return new GetCompanyResponse
            {
                Id = company.Id,
                OwnerID = company.OwnerID,
                Name = company.Name,
                Description = company.Description,
                Logo = company.Logo,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt,
                CountryOfCitizenship = company.CountryOfCitizenship,
                FullBirthName = company.FullBirthName,
                CountryOfBirth = company.CountryOfBirth,
                BirthDate = company.BirthDate,
                IdentityDocumentType = company.IdentityDocumentType,
                IdentityDocument = company.IdentityDocument,
                BankDocument = company.BankDocument,
                FacturationDocument = company.FacturationDocument,
                CompanyPhone = company.CompanyPhone,
                CompanyType = company.CompanyType,
                Email = company.Email,
                WebSite = company.WebSite,
                Address1 = company.Address1,
                Address2 = company.Address2,
                Address3 = company.Address3,
                City = company.City,
                ProvinceState = company.ProvinceState,
                Country = company.Country,
                PostalCode = company.PostalCode
            };
        }

        public static UpdateCompanyResponse ConvertToUpdateCompanyResponse(this Company company)
        {
            return new UpdateCompanyResponse
            {
                Id = company.Id,
                OwnerID = company.OwnerID,
                Name = company.Name,
                Description = company.Description,
                Logo = company.Logo,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt,
                CountryOfCitizenship = company.CountryOfCitizenship,
                FullBirthName = company.FullBirthName,
                CountryOfBirth = company.CountryOfBirth,
                BirthDate = company.BirthDate,
                IdentityDocumentType = company.IdentityDocumentType,
                IdentityDocument = company.IdentityDocument,
                BankDocument = company.BankDocument,
                FacturationDocument = company.FacturationDocument,
                CompanyPhone = company.CompanyPhone,
                CompanyType = company.CompanyType,
                Email = company.Email,
                WebSite = company.WebSite,
                Address1 = company.Address1,
                Address2 = company.Address2,
                Address3 = company.Address3,
                City = company.City,
                ProvinceState = company.ProvinceState,
                Country = company.Country,
                PostalCode = company.PostalCode
            };
        }

        public static DeleteCompanyResponse ConvertToDeleteCompanyResponse(this Company company)
        {
            return new DeleteCompanyResponse
            {
                Id = company.Id,
                Name = company.Name,
                DeletedAt = DateTime.UtcNow
            };
        }
    }
}
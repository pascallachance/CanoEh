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
                UpdatedAt = company.UpdatedAt
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
                UpdatedAt = company.UpdatedAt
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
                UpdatedAt = company.UpdatedAt
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
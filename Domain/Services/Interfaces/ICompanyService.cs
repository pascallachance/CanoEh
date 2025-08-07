using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;
using Infrastructure.Data;

namespace Domain.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<Result<CreateCompanyResponse>> CreateCompanyAsync(CreateCompanyRequest newCompany, Guid ownerId);
        Task<Result<GetCompanyResponse>> GetCompanyAsync(Guid companyId);
        Task<Result<IEnumerable<GetCompanyResponse>>> GetCompaniesByOwnerAsync(Guid ownerId);
        Task<Result<UpdateCompanyResponse>> UpdateCompanyAsync(UpdateCompanyRequest updateRequest);
        Task<Result<DeleteCompanyResponse>> DeleteCompanyAsync(Guid companyId, Guid ownerId);
        Task<Result<Company?>> GetCompanyEntityAsync(Guid companyId);
    }
}
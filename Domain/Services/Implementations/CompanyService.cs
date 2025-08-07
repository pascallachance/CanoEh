using System.Diagnostics;
using Domain.Models.Converters;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class CompanyService(ICompanyRepository companyRepository, IUserRepository userRepository) : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository = companyRepository;
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<Result<CreateCompanyResponse>> CreateCompanyAsync(CreateCompanyRequest newCompany, Guid ownerId)
        {
            try
            {
                var validationResult = newCompany.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<CreateCompanyResponse>(
                        validationResult.Error ?? "Validation failed.", 
                        validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                    );
                }

                if (ownerId == Guid.Empty)
                {
                    return Result.Failure<CreateCompanyResponse>("Owner ID is required.", StatusCodes.Status400BadRequest);
                }

                // Check if the owner exists
                var ownerExists = await _userRepository.ExistsAsync(ownerId);
                if (!ownerExists)
                {
                    return Result.Failure<CreateCompanyResponse>("Owner not found.", StatusCodes.Status400BadRequest);
                }

                // Check if company name already exists
                var existingCompany = await _companyRepository.FindByNameAsync(newCompany.Name);
                if (existingCompany != null)
                {
                    return Result.Failure<CreateCompanyResponse>("Company name already exists.", StatusCodes.Status400BadRequest);
                }

                var company = await _companyRepository.AddAsync(new Company
                {
                    OwnerID = ownerId,
                    Name = newCompany.Name,
                    Description = newCompany.Description,
                    Logo = newCompany.Logo,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                });

                CreateCompanyResponse createdCompany = company.ConvertToCreateCompanyResponse();

                Debug.WriteLine($"Company {newCompany.Name} created successfully.");
                return Result.Success(createdCompany);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating company: {ex.Message}");
                return Result.Failure<CreateCompanyResponse>("Failed to create company.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<GetCompanyResponse>> GetCompanyAsync(Guid companyId)
        {
            try
            {
                if (companyId == Guid.Empty)
                {
                    return Result.Failure<GetCompanyResponse>("Company ID is required.", StatusCodes.Status400BadRequest);
                }

                var companyFound = await _companyRepository.GetByIdAsync(companyId);
                if (companyFound == null)
                {
                    return Result.Failure<GetCompanyResponse>("Company not found.", StatusCodes.Status404NotFound);
                }

                GetCompanyResponse companyResponse = companyFound.ConvertToGetCompanyResponse();
                return Result.Success(companyResponse);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting company: {ex.Message}");
                return Result.Failure<GetCompanyResponse>("Failed to get company.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetCompanyResponse>>> GetCompaniesByOwnerAsync(Guid ownerId)
        {
            try
            {
                if (ownerId == Guid.Empty)
                {
                    return Result.Failure<IEnumerable<GetCompanyResponse>>("Owner ID is required.", StatusCodes.Status400BadRequest);
                }

                var companies = await _companyRepository.FindByOwnerAsync(ownerId);
                var companyResponses = companies.Select(c => c.ConvertToGetCompanyResponse());

                return Result.Success(companyResponses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting companies by owner: {ex.Message}");
                return Result.Failure<IEnumerable<GetCompanyResponse>>("Failed to get companies.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<UpdateCompanyResponse>> UpdateCompanyAsync(UpdateCompanyRequest updateRequest)
        {
            try
            {
                var validationResult = updateRequest.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<UpdateCompanyResponse>(
                        validationResult.Error ?? "Validation failed.", 
                        validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                    );
                }

                // Find the company to update
                var companyToUpdate = await _companyRepository.GetByIdAsync(updateRequest.Id);
                if (companyToUpdate == null)
                {
                    return Result.Failure<UpdateCompanyResponse>("Company not found.", StatusCodes.Status404NotFound);
                }

                // Check if the user is the owner
                if (companyToUpdate.OwnerID != updateRequest.OwnerID)
                {
                    return Result.Failure<UpdateCompanyResponse>("You are not authorized to update this company.", StatusCodes.Status403Forbidden);
                }

                // Check if new name already exists (excluding current company)
                if (companyToUpdate.Name != updateRequest.Name)
                {
                    var existingCompany = await _companyRepository.FindByNameAsync(updateRequest.Name);
                    if (existingCompany != null && existingCompany.Id != updateRequest.Id)
                    {
                        return Result.Failure<UpdateCompanyResponse>("Company name already exists.", StatusCodes.Status400BadRequest);
                    }
                }

                // Update the company
                companyToUpdate.Name = updateRequest.Name;
                companyToUpdate.Description = updateRequest.Description;
                companyToUpdate.Logo = updateRequest.Logo;
                companyToUpdate.UpdatedAt = DateTime.UtcNow;

                var updatedCompany = await _companyRepository.UpdateAsync(companyToUpdate);

                UpdateCompanyResponse response = updatedCompany.ConvertToUpdateCompanyResponse();

                Debug.WriteLine($"Company {updateRequest.Name} updated successfully.");
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating company: {ex.Message}");
                return Result.Failure<UpdateCompanyResponse>("Failed to update company.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<DeleteCompanyResponse>> DeleteCompanyAsync(Guid companyId, Guid ownerId)
        {
            try
            {
                if (companyId == Guid.Empty)
                {
                    return Result.Failure<DeleteCompanyResponse>("Company ID is required.", StatusCodes.Status400BadRequest);
                }

                if (ownerId == Guid.Empty)
                {
                    return Result.Failure<DeleteCompanyResponse>("Owner ID is required.", StatusCodes.Status400BadRequest);
                }

                // Find the company to delete
                var companyToDelete = await _companyRepository.GetByIdAsync(companyId);
                if (companyToDelete == null)
                {
                    return Result.Failure<DeleteCompanyResponse>("Company not found.", StatusCodes.Status404NotFound);
                }

                // Check if the user is the owner
                if (companyToDelete.OwnerID != ownerId)
                {
                    return Result.Failure<DeleteCompanyResponse>("You are not authorized to delete this company.", StatusCodes.Status403Forbidden);
                }

                // Delete the company
                await _companyRepository.DeleteAsync(companyToDelete);

                DeleteCompanyResponse response = companyToDelete.ConvertToDeleteCompanyResponse();

                Debug.WriteLine($"Company {companyToDelete.Name} deleted successfully.");
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting company: {ex.Message}");
                return Result.Failure<DeleteCompanyResponse>("Failed to delete company.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<Company?>> GetCompanyEntityAsync(Guid companyId)
        {
            try
            {
                if (companyId == Guid.Empty)
                {
                    return Result.Failure<Company?>("Company ID is required.", StatusCodes.Status400BadRequest);
                }

                var companyFound = await _companyRepository.GetByIdAsync(companyId);
                return Result.Success<Company?>(companyFound);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting company entity: {ex.Message}");
                return Result.Success<Company?>(null);
            }
        }
    }
}
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class AddressService(IAddressRepository addressRepository) : IAddressService
    {
        private readonly IAddressRepository _addressRepository = addressRepository;

        public async Task<Result<CreateAddressResponse>> CreateAddressAsync(CreateAddressRequest request, Guid userId)
        {
            try
            {
                var validationResult = request.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<CreateAddressResponse>(
                        validationResult.Error ?? "Validation failed.", 
                        validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                    );
                }

                var address = new Address
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FullName = request.FullName,
                    AddressLine1 = request.AddressLine1,
                    AddressLine2 = request.AddressLine2,
                    AddressLine3 = request.AddressLine3,
                    City = request.City,
                    ProvinceState = request.ProvinceState,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    AddressType = request.AddressType,
                    IsDefault = request.IsDefault,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };

                var createdAddress = await _addressRepository.AddAsync(address);

                var response = new CreateAddressResponse
                {
                    Id = createdAddress.Id,
                    UserId = createdAddress.UserId,
                    FullName = createdAddress.FullName,
                    AddressLine1 = createdAddress.AddressLine1,
                    AddressLine2 = createdAddress.AddressLine2,
                    AddressLine3 = createdAddress.AddressLine3,
                    City = createdAddress.City,
                    ProvinceState = createdAddress.ProvinceState,
                    PostalCode = createdAddress.PostalCode,
                    Country = createdAddress.Country,
                    AddressType = createdAddress.AddressType,
                    IsDefault = createdAddress.IsDefault,
                    CreatedAt = createdAddress.CreatedAt,
                    UpdatedAt = createdAddress.UpdatedAt
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<CreateAddressResponse>($"Error creating address: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<UpdateAddressResponse>> UpdateAddressAsync(UpdateAddressRequest request, Guid userId)
        {
            try
            {
                var validationResult = request.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<UpdateAddressResponse>(
                        validationResult.Error ?? "Validation failed.", 
                        validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                    );
                }

                // Check if the address exists and belongs to the user
                var addressExists = await _addressRepository.ExistsByUserIdAsync(userId, request.Id);
                if (!addressExists)
                {
                    return Result.Failure<UpdateAddressResponse>("Address not found or you don't have permission to update it.", StatusCodes.Status404NotFound);
                }

                var existingAddress = await _addressRepository.GetByIdAsync(request.Id);
                if (existingAddress == null)
                {
                    return Result.Failure<UpdateAddressResponse>("Address not found.", StatusCodes.Status404NotFound);
                }

                existingAddress.FullName = request.FullName;
                existingAddress.AddressLine1 = request.AddressLine1;
                existingAddress.AddressLine2 = request.AddressLine2;
                existingAddress.AddressLine3 = request.AddressLine3;
                existingAddress.City = request.City;
                existingAddress.ProvinceState = request.ProvinceState;
                existingAddress.PostalCode = request.PostalCode;
                existingAddress.Country = request.Country;
                existingAddress.AddressType = request.AddressType;
                existingAddress.IsDefault = request.IsDefault;
                existingAddress.UpdatedAt = DateTime.UtcNow;

                var updatedAddress = await _addressRepository.UpdateAsync(existingAddress);

                var response = new UpdateAddressResponse
                {
                    Id = updatedAddress.Id,
                    UserId = updatedAddress.UserId,
                    FullName = updatedAddress.FullName,
                    AddressLine1 = updatedAddress.AddressLine1,
                    AddressLine2 = updatedAddress.AddressLine2,
                    AddressLine3 = updatedAddress.AddressLine3,
                    City = updatedAddress.City,
                    ProvinceState = updatedAddress.ProvinceState,
                    PostalCode = updatedAddress.PostalCode,
                    Country = updatedAddress.Country,
                    AddressType = updatedAddress.AddressType,
                    IsDefault = updatedAddress.IsDefault,
                    CreatedAt = updatedAddress.CreatedAt,
                    UpdatedAt = updatedAddress.UpdatedAt
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<UpdateAddressResponse>($"Error updating address: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<DeleteAddressResponse>> DeleteAddressAsync(Guid addressId, Guid userId)
        {
            try
            {
                // Check if the address exists and belongs to the user
                var addressExists = await _addressRepository.ExistsByUserIdAsync(userId, addressId);
                if (!addressExists)
                {
                    return Result.Failure<DeleteAddressResponse>("Address not found or you don't have permission to delete it.", StatusCodes.Status404NotFound);
                }

                var address = await _addressRepository.GetByIdAsync(addressId);
                if (address == null)
                {
                    return Result.Failure<DeleteAddressResponse>("Address not found.", StatusCodes.Status404NotFound);
                }

                await _addressRepository.DeleteAsync(address);

                var response = new DeleteAddressResponse
                {
                    Id = addressId
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<DeleteAddressResponse>($"Error deleting address: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<GetAddressResponse>> GetAddressAsync(Guid addressId, Guid userId)
        {
            try
            {
                // Check if the address exists and belongs to the user
                var addressExists = await _addressRepository.ExistsByUserIdAsync(userId, addressId);
                if (!addressExists)
                {
                    return Result.Failure<GetAddressResponse>("Address not found or you don't have permission to access it.", StatusCodes.Status404NotFound);
                }

                var address = await _addressRepository.GetByIdAsync(addressId);
                if (address == null)
                {
                    return Result.Failure<GetAddressResponse>("Address not found.", StatusCodes.Status404NotFound);
                }

                var response = new GetAddressResponse
                {
                    Id = address.Id,
                    UserId = address.UserId,
                    FullName = address.FullName,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    AddressLine3 = address.AddressLine3,
                    City = address.City,
                    ProvinceState = address.ProvinceState,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    AddressType = address.AddressType,
                    IsDefault = address.IsDefault,
                    CreatedAt = address.CreatedAt,
                    UpdatedAt = address.UpdatedAt
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<GetAddressResponse>($"Error retrieving address: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetAddressResponse>>> GetUserAddressesAsync(Guid userId)
        {
            try
            {
                var addresses = await _addressRepository.GetByUserIdAsync(userId);

                var response = addresses.Select(address => new GetAddressResponse
                {
                    Id = address.Id,
                    UserId = address.UserId,
                    FullName = address.FullName,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    AddressLine3 = address.AddressLine3,
                    City = address.City,
                    ProvinceState = address.ProvinceState,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    AddressType = address.AddressType,
                    IsDefault = address.IsDefault,
                    CreatedAt = address.CreatedAt,
                    UpdatedAt = address.UpdatedAt
                });

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetAddressResponse>>($"Error retrieving addresses: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetAddressResponse>>> GetUserAddressesByTypeAsync(Guid userId, string addressType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(addressType))
                {
                    return Result.Failure<IEnumerable<GetAddressResponse>>("Address type is required.", StatusCodes.Status400BadRequest);
                }

                var addresses = await _addressRepository.GetByUserIdAndTypeAsync(userId, addressType);

                var response = addresses.Select(address => new GetAddressResponse
                {
                    Id = address.Id,
                    UserId = address.UserId,
                    FullName = address.FullName,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    AddressLine3 = address.AddressLine3,
                    City = address.City,
                    ProvinceState = address.ProvinceState,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    AddressType = address.AddressType,
                    IsDefault = address.IsDefault,
                    CreatedAt = address.CreatedAt,
                    UpdatedAt = address.UpdatedAt
                });

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetAddressResponse>>($"Error retrieving addresses by type: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }
    }
}
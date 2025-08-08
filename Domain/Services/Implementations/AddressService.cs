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
                    Street = request.Street,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    AddressType = request.AddressType,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };

                var createdAddress = await _addressRepository.AddAsync(address);

                var response = new CreateAddressResponse
                {
                    Id = createdAddress.Id,
                    UserId = createdAddress.UserId,
                    Street = createdAddress.Street,
                    City = createdAddress.City,
                    State = createdAddress.State,
                    PostalCode = createdAddress.PostalCode,
                    Country = createdAddress.Country,
                    AddressType = createdAddress.AddressType,
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

                existingAddress.Street = request.Street;
                existingAddress.City = request.City;
                existingAddress.State = request.State;
                existingAddress.PostalCode = request.PostalCode;
                existingAddress.Country = request.Country;
                existingAddress.AddressType = request.AddressType;
                existingAddress.UpdatedAt = DateTime.UtcNow;

                var updatedAddress = await _addressRepository.UpdateAsync(existingAddress);

                var response = new UpdateAddressResponse
                {
                    Id = updatedAddress.Id,
                    UserId = updatedAddress.UserId,
                    Street = updatedAddress.Street,
                    City = updatedAddress.City,
                    State = updatedAddress.State,
                    PostalCode = updatedAddress.PostalCode,
                    Country = updatedAddress.Country,
                    AddressType = updatedAddress.AddressType,
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
                    Street = address.Street,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    AddressType = address.AddressType,
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
                    Street = address.Street,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    AddressType = address.AddressType,
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
                    Street = address.Street,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    AddressType = address.AddressType,
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
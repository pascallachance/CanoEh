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
    public class PaymentMethodService(IPaymentMethodRepository paymentMethodRepository, IUserRepository userRepository) : IPaymentMethodService
    {
        private readonly IPaymentMethodRepository _paymentMethodRepository = paymentMethodRepository;
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<Result<CreatePaymentMethodResponse>> CreatePaymentMethodAsync(Guid userId, CreatePaymentMethodRequest createRequest)
        {
            try
            {
                var validationResult = createRequest.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<CreatePaymentMethodResponse>(
                        validationResult.Error ?? "Validation failed.",
                        validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                    );
                }

                // Verify user exists
                var userExists = await _userRepository.ExistsAsync(userId);
                if (!userExists)
                {
                    return Result.Failure<CreatePaymentMethodResponse>("User not found.", StatusCodes.Status404NotFound);
                }

                // If this is set as default, clear other defaults first
                if (createRequest.IsDefault)
                {
                    await _paymentMethodRepository.ClearDefaultPaymentMethodsAsync(userId);
                }

                var paymentMethod = new PaymentMethod
                {
                    ID = Guid.NewGuid(),
                    UserID = userId,
                    Type = createRequest.Type,
                    CardHolderName = createRequest.CardHolderName,
                    CardLast4 = createRequest.CardLast4,
                    CardBrand = createRequest.CardBrand,
                    ExpirationMonth = createRequest.ExpirationMonth,
                    ExpirationYear = createRequest.ExpirationYear,
                    BillingAddress = createRequest.BillingAddress,
                    IsDefault = createRequest.IsDefault,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    IsActive = true
                };

                var createdPaymentMethod = await _paymentMethodRepository.AddAsync(paymentMethod);
                var response = createdPaymentMethod.ConvertToCreatePaymentMethodResponse();

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating payment method: {ex.Message}");
                return Result.Failure<CreatePaymentMethodResponse>("An error occurred while creating the payment method.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<GetPaymentMethodResponse>> GetPaymentMethodAsync(Guid userId, Guid paymentMethodId)
        {
            try
            {
                var paymentMethod = await _paymentMethodRepository.FindByUserIdAndIdAsync(userId, paymentMethodId);
                if (paymentMethod == null)
                {
                    return Result.Failure<GetPaymentMethodResponse>("Payment method not found.", StatusCodes.Status404NotFound);
                }

                var response = paymentMethod.ConvertToGetPaymentMethodResponse();
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting payment method: {ex.Message}");
                return Result.Failure<GetPaymentMethodResponse>("An error occurred while retrieving the payment method.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetPaymentMethodResponse>>> GetUserPaymentMethodsAsync(Guid userId)
        {
            try
            {
                var paymentMethods = await _paymentMethodRepository.FindByUserIdAsync(userId);
                var response = paymentMethods.Select(pm => pm.ConvertToGetPaymentMethodResponse());
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting user payment methods: {ex.Message}");
                return Result.Failure<IEnumerable<GetPaymentMethodResponse>>("An error occurred while retrieving payment methods.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetPaymentMethodResponse>>> GetActiveUserPaymentMethodsAsync(Guid userId)
        {
            try
            {
                var paymentMethods = await _paymentMethodRepository.FindActiveByUserIdAsync(userId);
                var response = paymentMethods.Select(pm => pm.ConvertToGetPaymentMethodResponse());
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting active user payment methods: {ex.Message}");
                return Result.Failure<IEnumerable<GetPaymentMethodResponse>>("An error occurred while retrieving active payment methods.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<UpdatePaymentMethodResponse>> UpdatePaymentMethodAsync(Guid userId, UpdatePaymentMethodRequest updateRequest)
        {
            try
            {
                var validationResult = updateRequest.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<UpdatePaymentMethodResponse>(
                        validationResult.Error ?? "Validation failed.",
                        validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                    );
                }

                var existingPaymentMethod = await _paymentMethodRepository.FindByUserIdAndIdAsync(userId, updateRequest.ID);
                if (existingPaymentMethod == null)
                {
                    return Result.Failure<UpdatePaymentMethodResponse>("Payment method not found.", StatusCodes.Status404NotFound);
                }

                // If this is set as default, clear other defaults first
                if (updateRequest.IsDefault && !existingPaymentMethod.IsDefault)
                {
                    await _paymentMethodRepository.ClearDefaultPaymentMethodsAsync(userId);
                }

                existingPaymentMethod.Type = updateRequest.Type;
                existingPaymentMethod.CardHolderName = updateRequest.CardHolderName;
                existingPaymentMethod.CardLast4 = updateRequest.CardLast4;
                existingPaymentMethod.CardBrand = updateRequest.CardBrand;
                existingPaymentMethod.ExpirationMonth = updateRequest.ExpirationMonth;
                existingPaymentMethod.ExpirationYear = updateRequest.ExpirationYear;
                existingPaymentMethod.BillingAddress = updateRequest.BillingAddress;
                existingPaymentMethod.IsDefault = updateRequest.IsDefault;
                existingPaymentMethod.IsActive = updateRequest.IsActive;
                existingPaymentMethod.UpdatedAt = DateTime.UtcNow;

                var updatedPaymentMethod = await _paymentMethodRepository.UpdateAsync(existingPaymentMethod);
                var response = updatedPaymentMethod.ConvertToUpdatePaymentMethodResponse();

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating payment method: {ex.Message}");
                return Result.Failure<UpdatePaymentMethodResponse>("An error occurred while updating the payment method.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<DeletePaymentMethodResponse>> DeletePaymentMethodAsync(Guid userId, Guid paymentMethodId)
        {
            try
            {
                var paymentMethod = await _paymentMethodRepository.FindByUserIdAndIdAsync(userId, paymentMethodId);
                if (paymentMethod == null)
                {
                    return Result.Failure<DeletePaymentMethodResponse>("Payment method not found.", StatusCodes.Status404NotFound);
                }

                await _paymentMethodRepository.DeleteAsync(paymentMethod);

                var response = new DeletePaymentMethodResponse
                {
                    Success = true,
                    Message = "Payment method deleted successfully.",
                    PaymentMethodID = paymentMethodId
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting payment method: {ex.Message}");
                return Result.Failure<DeletePaymentMethodResponse>("An error occurred while deleting the payment method.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<GetPaymentMethodResponse>> SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId)
        {
            try
            {
                var paymentMethod = await _paymentMethodRepository.FindByUserIdAndIdAsync(userId, paymentMethodId);
                if (paymentMethod == null)
                {
                    return Result.Failure<GetPaymentMethodResponse>("Payment method not found.", StatusCodes.Status404NotFound);
                }

                if (!paymentMethod.IsActive)
                {
                    return Result.Failure<GetPaymentMethodResponse>("Cannot set inactive payment method as default.", StatusCodes.Status400BadRequest);
                }

                var success = await _paymentMethodRepository.SetDefaultPaymentMethodAsync(userId, paymentMethodId);
                if (!success)
                {
                    return Result.Failure<GetPaymentMethodResponse>("Failed to set payment method as default.", StatusCodes.Status500InternalServerError);
                }

                // Get the updated payment method
                var updatedPaymentMethod = await _paymentMethodRepository.FindByUserIdAndIdAsync(userId, paymentMethodId);
                var response = updatedPaymentMethod!.ConvertToGetPaymentMethodResponse();

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting default payment method: {ex.Message}");
                return Result.Failure<GetPaymentMethodResponse>("An error occurred while setting the default payment method.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<GetPaymentMethodResponse>> GetDefaultPaymentMethodAsync(Guid userId)
        {
            try
            {
                var paymentMethod = await _paymentMethodRepository.FindDefaultByUserIdAsync(userId);
                if (paymentMethod == null)
                {
                    return Result.Failure<GetPaymentMethodResponse>("No default payment method found.", StatusCodes.Status404NotFound);
                }

                var response = paymentMethod.ConvertToGetPaymentMethodResponse();
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting default payment method: {ex.Message}");
                return Result.Failure<GetPaymentMethodResponse>("An error occurred while retrieving the default payment method.", StatusCodes.Status500InternalServerError);
            }
        }
    }
}
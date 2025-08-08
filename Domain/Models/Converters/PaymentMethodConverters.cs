using Domain.Models.Responses;
using Infrastructure.Data;

namespace Domain.Models.Converters
{
    public static class PaymentMethodConverters
    {
        public static CreatePaymentMethodResponse ConvertToCreatePaymentMethodResponse(this PaymentMethod paymentMethod)
        {
            return new CreatePaymentMethodResponse
            {
                ID = paymentMethod.ID,
                UserID = paymentMethod.UserID,
                Type = paymentMethod.Type,
                CardHolderName = paymentMethod.CardHolderName,
                CardLast4 = paymentMethod.CardLast4,
                CardBrand = paymentMethod.CardBrand,
                ExpirationMonth = paymentMethod.ExpirationMonth,
                ExpirationYear = paymentMethod.ExpirationYear,
                BillingAddress = paymentMethod.BillingAddress,
                IsDefault = paymentMethod.IsDefault,
                CreatedAt = paymentMethod.CreatedAt,
                UpdatedAt = paymentMethod.UpdatedAt,
                IsActive = paymentMethod.IsActive
            };
        }

        public static GetPaymentMethodResponse ConvertToGetPaymentMethodResponse(this PaymentMethod paymentMethod)
        {
            return new GetPaymentMethodResponse
            {
                ID = paymentMethod.ID,
                UserID = paymentMethod.UserID,
                Type = paymentMethod.Type,
                CardHolderName = paymentMethod.CardHolderName,
                CardLast4 = paymentMethod.CardLast4,
                CardBrand = paymentMethod.CardBrand,
                ExpirationMonth = paymentMethod.ExpirationMonth,
                ExpirationYear = paymentMethod.ExpirationYear,
                BillingAddress = paymentMethod.BillingAddress,
                IsDefault = paymentMethod.IsDefault,
                CreatedAt = paymentMethod.CreatedAt,
                UpdatedAt = paymentMethod.UpdatedAt,
                IsActive = paymentMethod.IsActive
            };
        }

        public static UpdatePaymentMethodResponse ConvertToUpdatePaymentMethodResponse(this PaymentMethod paymentMethod)
        {
            return new UpdatePaymentMethodResponse
            {
                ID = paymentMethod.ID,
                UserID = paymentMethod.UserID,
                Type = paymentMethod.Type,
                CardHolderName = paymentMethod.CardHolderName,
                CardLast4 = paymentMethod.CardLast4,
                CardBrand = paymentMethod.CardBrand,
                ExpirationMonth = paymentMethod.ExpirationMonth,
                ExpirationYear = paymentMethod.ExpirationYear,
                BillingAddress = paymentMethod.BillingAddress,
                IsDefault = paymentMethod.IsDefault,
                CreatedAt = paymentMethod.CreatedAt,
                UpdatedAt = paymentMethod.UpdatedAt,
                IsActive = paymentMethod.IsActive
            };
        }
    }
}
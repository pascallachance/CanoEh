using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreateItemVariantAttributeRequest
    {
        public string AttributeName_en { get; set; } = string.Empty;
        public string? AttributeName_fr { get; set; }
        public string Attributes_en { get; set; } = string.Empty;
        public string? Attributes_fr { get; set; }

        public Result Validate()
        {
            if (AttributeName_en != null && AttributeName_en.Length > 255)
            {
                return Result.Failure("Attribute name (English) cannot exceed 255 characters.", StatusCodes.Status400BadRequest);
            }

            if (AttributeName_fr != null && AttributeName_fr.Length > 255)
            {
                return Result.Failure("Attribute name (French) cannot exceed 255 characters.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }
}

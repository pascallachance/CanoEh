using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class UpdateVariantImageUrlsRequest
    {
        public Guid VariantId { get; set; }
        public string? ThumbnailUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();

        public Result Validate()
        {
            if (VariantId == Guid.Empty)
            {
                return Result.Failure("VariantId is required.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }
}

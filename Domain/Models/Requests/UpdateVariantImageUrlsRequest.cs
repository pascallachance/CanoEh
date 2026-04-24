using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class UpdateVariantImageUrlsRequest
    {
        // Maximum number of images per variant (must match the upload endpoint's limit)
        private const int MaxImageCount = 10;
        // Maximum length for a single URL to guard against oversized data
        private const int MaxUrlLength = 2048;

        public Guid VariantId { get; set; }
        public string? ThumbnailUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public string? VideoUrl { get; set; }

        public Result Validate()
        {
            if (VariantId == Guid.Empty)
            {
                return Result.Failure("VariantId is required.", StatusCodes.Status400BadRequest);
            }

            if (ImageUrls.Count > MaxImageCount)
            {
                return Result.Failure($"A maximum of {MaxImageCount} images are allowed per variant.", StatusCodes.Status400BadRequest);
            }

            foreach (var url in ImageUrls)
            {
                if (url != null && url.Length > MaxUrlLength)
                {
                    return Result.Failure($"One or more image URLs exceed the maximum allowed length of {MaxUrlLength} characters.", StatusCodes.Status400BadRequest);
                }
            }

            if (ThumbnailUrl != null && ThumbnailUrl.Length > MaxUrlLength)
            {
                return Result.Failure($"Thumbnail URL exceeds the maximum allowed length of {MaxUrlLength} characters.", StatusCodes.Status400BadRequest);
            }

            if (VideoUrl != null && VideoUrl.Length > MaxUrlLength)
            {
                return Result.Failure($"Video URL exceeds the maximum allowed length of {MaxUrlLength} characters.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }
}

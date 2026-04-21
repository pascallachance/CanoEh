using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class UpdateItemReviewRequest
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string? ReviewText { get; set; }

        public Result Validate()
        {
            if (Id == Guid.Empty)
            {
                return Result.Failure("Review ID is required.", StatusCodes.Status400BadRequest);
            }

            if (Rating < 0 || Rating > 5)
            {
                return Result.Failure("Rating must be between 0 and 5 maple leaves.", StatusCodes.Status400BadRequest);
            }

            if (ReviewText?.Length > 2000)
            {
                return Result.Failure("Review text cannot exceed 2000 characters.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }
}

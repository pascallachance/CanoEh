using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Exceptions;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class ItemReviewService(IItemReviewRepository itemReviewRepository, IItemRepository itemRepository) : IItemReviewService
    {
        private readonly IItemReviewRepository _itemReviewRepository = itemReviewRepository;
        private readonly IItemRepository _itemRepository = itemRepository;

        public async Task<Result<CreateItemReviewResponse>> CreateItemReviewAsync(Guid userId, CreateItemReviewRequest request)
        {
            try
            {
                var validation = request.Validate();
                if (validation.IsFailure)
                {
                    return Result.Failure<CreateItemReviewResponse>(validation.Error!, validation.ErrorCode ?? StatusCodes.Status400BadRequest);
                }

                var item = await _itemRepository.GetItemByIdAsync(request.ItemID);
                if (item == null)
                {
                    return Result.Failure<CreateItemReviewResponse>("Item not found.", StatusCodes.Status404NotFound);
                }

                var userPurchasedItem = await _itemReviewRepository.HasUserPurchasedItemAsync(userId, request.ItemID);
                if (!userPurchasedItem)
                {
                    return Result.Failure<CreateItemReviewResponse>("Only customers who purchased this product can rate it.", StatusCodes.Status403Forbidden);
                }

                var existingReview = await _itemReviewRepository.GetByUserAndItemAsync(userId, request.ItemID);
                if (existingReview != null)
                {
                    return Result.Failure<CreateItemReviewResponse>("You have already reviewed this product.", StatusCodes.Status409Conflict);
                }

                var entity = new ItemReview
                {
                    ItemID = request.ItemID,
                    UserID = userId,
                    Rating = request.Rating,
                    ReviewText = request.ReviewText,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };

                ItemReview created;
                try
                {
                    created = await _itemReviewRepository.AddAsync(entity);
                }
                catch (DuplicateItemReviewException)
                {
                    return Result.Failure<CreateItemReviewResponse>("You have already reviewed this product.", StatusCodes.Status409Conflict);
                }

                return Result.Success(new CreateItemReviewResponse
                {
                    Id = created.Id,
                    ItemID = created.ItemID,
                    UserID = created.UserID,
                    Rating = created.Rating,
                    ReviewText = created.ReviewText,
                    CreatedAt = created.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return Result.Failure<CreateItemReviewResponse>($"An error occurred while creating the review: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<GetItemReviewResponse>> GetItemReviewByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Result.Failure<GetItemReviewResponse>("Review ID is required.", StatusCodes.Status400BadRequest);
                }

                var exists = await _itemReviewRepository.ExistsAsync(id);
                if (!exists)
                {
                    return Result.Failure<GetItemReviewResponse>("Review not found.", StatusCodes.Status404NotFound);
                }

                var review = await _itemReviewRepository.GetByIdAsync(id);
                return Result.Success(Map(review));
            }
            catch (Exception ex)
            {
                return Result.Failure<GetItemReviewResponse>($"An error occurred while retrieving the review: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetItemReviewResponse>>> GetItemReviewsByItemIdAsync(Guid itemId)
        {
            try
            {
                if (itemId == Guid.Empty)
                {
                    return Result.Failure<IEnumerable<GetItemReviewResponse>>("Item ID is required.", StatusCodes.Status400BadRequest);
                }

                var reviews = await _itemReviewRepository.GetByItemIdAsync(itemId);
                return Result.Success(reviews.Select(Map));
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetItemReviewResponse>>($"An error occurred while retrieving item reviews: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<UpdateItemReviewResponse>> UpdateItemReviewAsync(Guid userId, UpdateItemReviewRequest request)
        {
            try
            {
                var validation = request.Validate();
                if (validation.IsFailure)
                {
                    return Result.Failure<UpdateItemReviewResponse>(validation.Error!, validation.ErrorCode ?? StatusCodes.Status400BadRequest);
                }

                var exists = await _itemReviewRepository.ExistsAsync(request.Id);
                if (!exists)
                {
                    return Result.Failure<UpdateItemReviewResponse>("Review not found.", StatusCodes.Status404NotFound);
                }

                var existing = await _itemReviewRepository.GetByIdAsync(request.Id);
                if (existing.UserID != userId)
                {
                    return Result.Failure<UpdateItemReviewResponse>("You can only update your own review.", StatusCodes.Status403Forbidden);
                }

                existing.Rating = request.Rating;
                existing.ReviewText = request.ReviewText;
                existing.UpdatedAt = DateTime.UtcNow;

                var updated = await _itemReviewRepository.UpdateAsync(existing);

                return Result.Success(new UpdateItemReviewResponse
                {
                    Id = updated.Id,
                    ItemID = updated.ItemID,
                    UserID = updated.UserID,
                    Rating = updated.Rating,
                    ReviewText = updated.ReviewText,
                    UpdatedAt = updated.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                return Result.Failure<UpdateItemReviewResponse>($"An error occurred while updating the review: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<DeleteItemReviewResponse>> DeleteItemReviewAsync(Guid userId, Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Result.Failure<DeleteItemReviewResponse>("Review ID is required.", StatusCodes.Status400BadRequest);
                }

                var exists = await _itemReviewRepository.ExistsAsync(id);
                if (!exists)
                {
                    return Result.Failure<DeleteItemReviewResponse>("Review not found.", StatusCodes.Status404NotFound);
                }

                var existing = await _itemReviewRepository.GetByIdAsync(id);
                if (existing.UserID != userId)
                {
                    return Result.Failure<DeleteItemReviewResponse>("You can only delete your own review.", StatusCodes.Status403Forbidden);
                }

                await _itemReviewRepository.DeleteAsync(existing);

                return Result.Success(new DeleteItemReviewResponse
                {
                    Id = id,
                    Message = "Review deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return Result.Failure<DeleteItemReviewResponse>($"An error occurred while deleting the review: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<ItemRatingSummaryResponse>> GetItemRatingSummaryAsync(Guid itemId)
        {
            try
            {
                if (itemId == Guid.Empty)
                {
                    return Result.Failure<ItemRatingSummaryResponse>("Item ID is required.", StatusCodes.Status400BadRequest);
                }

                var summary = (await _itemReviewRepository.GetRatingSummariesAsync([itemId])).FirstOrDefault();
                if (summary == null)
                {
                    return Result.Success(new ItemRatingSummaryResponse
                    {
                        ItemID = itemId,
                        AverageRating = 0,
                        RatingCount = 0
                    });
                }

                return Result.Success(new ItemRatingSummaryResponse
                {
                    ItemID = summary.ItemID,
                    AverageRating = summary.AverageRating,
                    RatingCount = summary.RatingCount
                });
            }
            catch (Exception ex)
            {
                return Result.Failure<ItemRatingSummaryResponse>($"An error occurred while retrieving rating summary: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<ReviewReminderCandidateResponse>>> GetPendingReviewReminderCandidatesAsync(DateTime cutoffUtc)
        {
            try
            {
                var candidates = await _itemReviewRepository.GetPendingReviewReminderCandidatesAsync(cutoffUtc);
                var response = candidates.Select(c => new ReviewReminderCandidateResponse
                {
                    UserID = c.UserID,
                    Email = c.Email,
                    ItemID = c.ItemID,
                    ItemName_en = c.ItemName_en,
                    ItemName_fr = c.ItemName_fr,
                    DeliveredAt = c.DeliveredAt
                });

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<ReviewReminderCandidateResponse>>($"An error occurred while retrieving review reminder candidates: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        private static GetItemReviewResponse Map(ItemReview review)
        {
            return new GetItemReviewResponse
            {
                Id = review.Id,
                ItemID = review.ItemID,
                Rating = review.Rating,
                ReviewText = review.ReviewText,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }
    }
}

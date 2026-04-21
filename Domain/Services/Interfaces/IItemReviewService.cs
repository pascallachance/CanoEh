using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;

namespace Domain.Services.Interfaces
{
    public interface IItemReviewService
    {
        Task<Result<CreateItemReviewResponse>> CreateItemReviewAsync(Guid userId, CreateItemReviewRequest request);
        Task<Result<GetItemReviewResponse>> GetItemReviewByIdAsync(Guid id);
        Task<Result<IEnumerable<GetItemReviewResponse>>> GetItemReviewsByItemIdAsync(Guid itemId);
        Task<Result<UpdateItemReviewResponse>> UpdateItemReviewAsync(Guid userId, UpdateItemReviewRequest request);
        Task<Result<DeleteItemReviewResponse>> DeleteItemReviewAsync(Guid userId, Guid id);
        Task<Result<ItemRatingSummaryResponse>> GetItemRatingSummaryAsync(Guid itemId);
        Task<Result<IEnumerable<ReviewReminderCandidateResponse>>> GetPendingReviewReminderCandidatesAsync(DateTime cutoffUtc);
    }
}

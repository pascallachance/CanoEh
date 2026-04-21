using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IItemReviewRepository : IRepository<ItemReview>
    {
        Task<ItemReview?> GetByUserAndItemAsync(Guid userId, Guid itemId);
        Task<IEnumerable<ItemReview>> GetByItemIdAsync(Guid itemId);
        Task<IEnumerable<ItemRatingSummary>> GetRatingSummariesAsync(IEnumerable<Guid> itemIds);
        Task<bool> HasUserPurchasedItemAsync(Guid userId, Guid itemId);
        Task<IEnumerable<ReviewReminderCandidate>> GetPendingReviewReminderCandidatesAsync(DateTime cutoffUtc);
    }
}

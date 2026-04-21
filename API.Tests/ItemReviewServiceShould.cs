using Domain.Models.Requests;
using Domain.Services.Implementations;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace API.Tests
{
    public class ItemReviewServiceShould
    {
        private readonly Mock<IItemReviewRepository> _mockItemReviewRepository;
        private readonly Mock<IItemRepository> _mockItemRepository;
        private readonly ItemReviewService _service;

        public ItemReviewServiceShould()
        {
            _mockItemReviewRepository = new Mock<IItemReviewRepository>();
            _mockItemRepository = new Mock<IItemRepository>();
            _service = new ItemReviewService(_mockItemReviewRepository.Object, _mockItemRepository.Object);
        }

        [Fact]
        public async Task CreateItemReviewAsync_ReturnForbidden_WhenUserDidNotPurchaseItem()
        {
            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var request = new CreateItemReviewRequest { ItemID = itemId, Rating = 4 };

            _mockItemRepository.Setup(x => x.GetItemByIdAsync(itemId))
                .ReturnsAsync(new Item { Id = itemId });
            _mockItemReviewRepository.Setup(x => x.HasUserPurchasedItemAsync(userId, itemId))
                .ReturnsAsync(false);

            var result = await _service.CreateItemReviewAsync(userId, request);

            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status403Forbidden, result.ErrorCode);
        }

        [Fact]
        public async Task CreateItemReviewAsync_ReturnConflict_WhenReviewAlreadyExists()
        {
            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var request = new CreateItemReviewRequest { ItemID = itemId, Rating = 5 };

            _mockItemRepository.Setup(x => x.GetItemByIdAsync(itemId))
                .ReturnsAsync(new Item { Id = itemId });
            _mockItemReviewRepository.Setup(x => x.HasUserPurchasedItemAsync(userId, itemId))
                .ReturnsAsync(true);
            _mockItemReviewRepository.Setup(x => x.GetByUserAndItemAsync(userId, itemId))
                .ReturnsAsync(new ItemReview { Id = Guid.NewGuid(), ItemID = itemId, UserID = userId });

            var result = await _service.CreateItemReviewAsync(userId, request);

            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status409Conflict, result.ErrorCode);
        }

        [Fact]
        public async Task UpdateItemReviewAsync_ReturnForbidden_WhenUserIsNotOwner()
        {
            var ownerId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var reviewId = Guid.NewGuid();

            _mockItemReviewRepository.Setup(x => x.ExistsAsync(reviewId)).ReturnsAsync(true);
            _mockItemReviewRepository.Setup(x => x.GetByIdAsync(reviewId))
                .ReturnsAsync(new ItemReview { Id = reviewId, UserID = ownerId, ItemID = Guid.NewGuid(), Rating = 3 });

            var result = await _service.UpdateItemReviewAsync(anotherUserId, new UpdateItemReviewRequest
            {
                Id = reviewId,
                Rating = 4,
                ReviewText = "Updated"
            });

            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status403Forbidden, result.ErrorCode);
        }

        [Fact]
        public async Task DeleteItemReviewAsync_ReturnForbidden_WhenUserIsNotOwner()
        {
            var ownerId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var reviewId = Guid.NewGuid();

            _mockItemReviewRepository.Setup(x => x.ExistsAsync(reviewId)).ReturnsAsync(true);
            _mockItemReviewRepository.Setup(x => x.GetByIdAsync(reviewId))
                .ReturnsAsync(new ItemReview { Id = reviewId, UserID = ownerId, ItemID = Guid.NewGuid(), Rating = 3 });

            var result = await _service.DeleteItemReviewAsync(anotherUserId, reviewId);

            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status403Forbidden, result.ErrorCode);
        }
    }
}

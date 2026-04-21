using API.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace API.Tests
{
    public class ItemReviewControllerShould
    {
        [Fact]
        public void GetPendingReviewReminderCandidates_HasAuthorizeAttribute()
        {
            var method = typeof(ItemReviewController).GetMethod(nameof(ItemReviewController.GetPendingReviewReminderCandidates));

            Assert.NotNull(method);
            var hasAuthorize = method!.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Any();
            var hasAllowAnonymous = method.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).Any();

            Assert.True(hasAuthorize);
            Assert.False(hasAllowAnonymous);
        }

        [Fact]
        public void GetItemReviewsByItem_AllowsAnonymous()
        {
            var method = typeof(ItemReviewController).GetMethod(nameof(ItemReviewController.GetItemReviewsByItem));

            Assert.NotNull(method);
            var hasAllowAnonymous = method!.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).Any();

            Assert.True(hasAllowAnonymous);
        }
    }
}

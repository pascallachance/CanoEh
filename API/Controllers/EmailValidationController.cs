using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class EmailValidationController : Controller
    {
        private readonly IUserService _userService;

        public EmailValidationController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("ValidateEmail/{userId}")]
        public async Task<IActionResult> Index(Guid userId)
        {
            var result = await _userService.ValidateEmailAsync(userId);
            ViewBag.Message = result.IsFailure ? GetUserFriendlyErrorMessage(result.Error) : "Email validated successfully!";
            return View("ValidateEmail");
        }
        private string GetUserFriendlyErrorMessage(string error)
        {
            // Map internal error messages to user-friendly messages
            return error switch
            {
                "InvalidUserId" => "The provided user ID is invalid.",
                "EmailNotFound" => "The email address could not be found.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }
    }
}
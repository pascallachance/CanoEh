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

        [HttpGet("ValidateEmail/{token}")]
        public async Task<IActionResult> Index(string token)
        {
            var result = await _userService.ValidateEmailByTokenAsync(token);
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
                "Validation token is required." => "Invalid validation link.",
                "Invalid or expired validation token." => "Invalid or expired validation link.",
                "User not found." => "Invalid validation link.",
                "Email is already validated." => "This email address has already been validated.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }
    }
}
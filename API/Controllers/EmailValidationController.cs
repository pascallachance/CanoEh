using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class EmailValidationController : Controller
    {
        // Error message constants for better maintainability
        private static class ErrorMessages
        {
            public const string InvalidUserId = "The provided user ID is invalid.";
            public const string EmailNotFound = "The email address could not be found.";
            public const string InvalidValidationLink = "Invalid validation link.";
            public const string InvalidOrExpiredValidationLink = "Invalid or expired validation link.";
            public const string EmailAlreadyValidated = "This email address has already been validated.";
            public const string UnexpectedError = "An unexpected error occurred. Please try again later.";
            public const string EmailValidatedSuccessfully = "Email validated successfully!";
        }

        private readonly IUserService _userService;

        public EmailValidationController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("ValidateEmail/{token}")]
        public async Task<IActionResult> Index(string token)
        {
            var result = await _userService.ValidateEmailByTokenAsync(token);
            ViewBag.Message = result.IsFailure ? GetUserFriendlyErrorMessage(result.Error) : ErrorMessages.EmailValidatedSuccessfully;
            return View("ValidateEmail");
        }
        private string GetUserFriendlyErrorMessage(string error)
        {
            // Map internal error messages to user-friendly messages
            return error switch
            {
                "InvalidUserId" => ErrorMessages.InvalidUserId,
                "EmailNotFound" => ErrorMessages.EmailNotFound,
                "Validation token is required." => ErrorMessages.InvalidValidationLink,
                "Invalid or expired validation token." => ErrorMessages.InvalidOrExpiredValidationLink,
                "User not found." => ErrorMessages.InvalidValidationLink,
                "Email is already validated." => ErrorMessages.EmailAlreadyValidated,
                _ => ErrorMessages.UnexpectedError
            };
        }
    }
}
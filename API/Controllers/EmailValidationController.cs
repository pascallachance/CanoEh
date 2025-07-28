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
            ViewBag.Message = result.IsFailure ? result.Error : "Email validated successfully!";
            return View("ValidateEmail");
        }
    }
}
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class ResetPasswordModel : PageModel
{
    [BindProperty]
    public string? Token { get; set; }

    [BindProperty]
    public string? NewPassword { get; set; }

    [BindProperty]
    public string? ConfirmNewPassword { get; set; }

    public string? Message { get; set; }
    public bool Success { get; set; }

    public void OnGet(string token)
    {
        Token = token;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Token) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmNewPassword))
        {
            Message = "All fields are required.";
            Success = false;
            return Page();
        }

        if (NewPassword != ConfirmNewPassword)
        {
            Message = "Passwords do not match.";
            Success = false;
            return Page();
        }

        var resetRequest = new
        {
            Token,
            NewPassword,
            ConfirmNewPassword
        };

        using var httpClient = new HttpClient();
        var content = new StringContent(JsonSerializer.Serialize(resetRequest), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("https://localhost:7182/api/PasswordReset/ResetPassword", content);

        if (response.IsSuccessStatusCode)
        {
            Message = "Your password has been reset successfully.";
            Success = true;
        }
        else
        {
            Message = $"Error: {await response.Content.ReadAsStringAsync()}";
            Success = false;
        }

        return Page();
    }
}
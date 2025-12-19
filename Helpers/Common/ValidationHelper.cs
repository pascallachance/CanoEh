namespace Helpers.Common
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates if a string is a valid email address format.
        /// </summary>
        /// <param name="email">The email address to validate</param>
        /// <returns>True if the email is valid, false otherwise</returns>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}

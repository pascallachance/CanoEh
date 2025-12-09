namespace Helpers.Common
{
    /// <summary>
    /// Helper class for generating multilingual email content
    /// </summary>
    public static class EmailContent
    {
        /// <summary>
        /// Get email validation subject and body based on user's language
        /// </summary>
        public static (string Subject, string Body) GetEmailValidation(string firstname, string lastname, string validationUrl, string? language)
        {
            var lang = language?.ToLowerInvariant() ?? "en";
            
            return lang switch
            {
                "fr" => GetEmailValidationFr(firstname, lastname, validationUrl),
                _ => GetEmailValidationEn(firstname, lastname, validationUrl)
            };
        }

        private static (string Subject, string Body) GetEmailValidationEn(string firstname, string lastname, string validationUrl)
        {
            var subject = "Email Validation";
            var body = $@"Hello {firstname} {lastname},

Thank you for registering with CanoEh! To complete your registration, please click the link below to validate your email address:

{validationUrl}

If you did not create this account, please ignore this email.

Best regards,
The CanoEh Team";
            
            return (subject, body);
        }

        private static (string Subject, string Body) GetEmailValidationFr(string firstname, string lastname, string validationUrl)
        {
            var subject = "Validation de l'adresse e-mail";
            var body = $@"Bonjour {firstname} {lastname},

Merci de vous être inscrit à CanoEh! Pour compléter votre inscription, veuillez cliquer sur le lien ci-dessous pour valider votre adresse e-mail:

{validationUrl}

Si vous n'avez pas créé ce compte, veuillez ignorer cet e-mail.

Cordialement,
L'équipe CanoEh";
            
            return (subject, body);
        }

        /// <summary>
        /// Get password reset subject and body based on user's language
        /// </summary>
        public static (string Subject, string Body) GetPasswordReset(string firstname, string lastname, string resetUrl, string? language)
        {
            var lang = language?.ToLowerInvariant() ?? "en";
            
            return lang switch
            {
                "fr" => GetPasswordResetFr(firstname, lastname, resetUrl),
                _ => GetPasswordResetEn(firstname, lastname, resetUrl)
            };
        }

        private static (string Subject, string Body) GetPasswordResetEn(string firstname, string lastname, string resetUrl)
        {
            var subject = "Password Reset Request";
            var body = $@"Hello {firstname} {lastname},

You have requested to reset your password for your CanoEh account. To reset your password, please click the link below:

{resetUrl}

This link will expire in 24 hours. If you did not request a password reset, please ignore this email.

Best regards,
The CanoEh Team";
            
            return (subject, body);
        }

        private static (string Subject, string Body) GetPasswordResetFr(string firstname, string lastname, string resetUrl)
        {
            var subject = "Demande de réinitialisation du mot de passe";
            var body = $@"Bonjour {firstname} {lastname},

Vous avez demandé de réinitialiser votre mot de passe pour votre compte CanoEh. Pour réinitialiser votre mot de passe, veuillez cliquer sur le lien ci-dessous:

{resetUrl}

Ce lien expirera dans 24 heures. Si vous n'avez pas demandé de réinitialisation du mot de passe, veuillez ignorer cet e-mail.

Cordialement,
L'équipe CanoEh";
            
            return (subject, body);
        }

        /// <summary>
        /// Get account restoration subject and body based on user's language
        /// </summary>
        public static (string Subject, string Body) GetRestoreUser(string firstname, string lastname, string restoreUrl, string? language)
        {
            var lang = language?.ToLowerInvariant() ?? "en";
            
            return lang switch
            {
                "fr" => GetRestoreUserFr(firstname, lastname, restoreUrl),
                _ => GetRestoreUserEn(firstname, lastname, restoreUrl)
            };
        }

        private static (string Subject, string Body) GetRestoreUserEn(string firstname, string lastname, string restoreUrl)
        {
            var subject = "Account Restoration Request";
            var body = $@"Hello {firstname} {lastname},

You have requested to restore your deleted CanoEh account. To restore your account, please click the link below:

{restoreUrl}

This link will expire in 24 hours. If you did not request account restoration, please ignore this email.

Best regards,
The CanoEh Team";
            
            return (subject, body);
        }

        private static (string Subject, string Body) GetRestoreUserFr(string firstname, string lastname, string restoreUrl)
        {
            var subject = "Demande de restauration de compte";
            var body = $@"Bonjour {firstname} {lastname},

Vous avez demandé de restaurer votre compte CanoEh supprimé. Pour restaurer votre compte, veuillez cliquer sur le lien ci-dessous:

{restoreUrl}

Ce lien expirera dans 24 heures. Si vous n'avez pas demandé de restauration de compte, veuillez ignorer cet e-mail.

Cordialement,
L'équipe CanoEh";
            
            return (subject, body);
        }
    }
}

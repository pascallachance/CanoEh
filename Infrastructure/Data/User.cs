namespace Infrastructure.Data
{
    public class User
    {
        public Guid ID { get; set; }
        public required string Uname { get; set; }
        public required string Firstname { get; set; }
        public required string Lastname { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? Lastlogin { get; set; }
        public DateTime? Lastlogout { get; set; }
        public DateTime Createdat { get; set; }
        public DateTime? Lastupdatedat { get; set; }
        public required string Password { get; set; }
        public bool Deleted { get; set; }
        public bool ValidEmail { get; set; }
        public string? EmailValidationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public string? RestoreUserToken { get; set; }
        public DateTime? RestoreUserTokenExpiry { get; set; }
    }
}

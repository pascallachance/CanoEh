namespace Infrastructure.Data
{
    public class Session
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LoggedOutAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }

        /// <summary>
        /// Indicates if the session is currently active (not logged out and not expired)
        /// </summary>
        public bool IsActive => LoggedOutAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
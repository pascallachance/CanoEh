namespace Domain.Models
{
    public class DeleteUserResponse
    {
        public Guid ID { get; set; }
        public required string Uname { get; set; }
        public required string Firstname { get; set; }
        public required string Lastname { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? Lastlogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastupdatedAt { get; set; }
        public bool Deleted { get; set; }
    }
}
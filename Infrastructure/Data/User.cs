namespace Infrastructure.Data
{
    public class User
    {
        public Guid id;
        public string uname;
        public string firstname;
        public string lastname;
        public string email;
        public string phone;
        public DateTime? lastlogin;
        public DateTime createdat;
        public DateTime? lastupdatedat;
        public string password;
        public bool deleted;
    }
}

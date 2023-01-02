namespace UserLoginApp.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public int Point { get; set; }

        public string? UserRole { get; set; }
    }
}

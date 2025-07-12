namespace NAQLAH.Server.Services
{
    public class UserSession
    {
        public UserSession()
        {
            this.Username = string.Empty;
            this.UserRole = string.Empty;
            this.PhoneNumber = string.Empty;
        }
        public string Username { get; set; }
        public int UserId { get; set; }
        public int LanguageId { get; set; }
        public string PhoneNumber { get; set; }
        public string UserRole { get; set; }
    }
}

using Domain.InterFaces;

namespace NAQLAH.Server.Services
{
    public class UserSessions: IUserSession
    {
        private readonly UserSession session;

        public UserSessions(UserSession session)
        {
            this.session = session;
        }

        public string Username => this.session.Username;

        public int UserId => this.session.UserId;

        public int LanguageId => this.session.LanguageId;
        public string PhoneNumber => this.session.PhoneNumber;
    }
}

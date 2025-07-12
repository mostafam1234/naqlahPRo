using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.InterFaces
{
    public interface IUserSession
    {
        string Username { get; }
        int UserId { get; }
        int LanguageId { get; }
        string PhoneNumber { get; }
    }
}

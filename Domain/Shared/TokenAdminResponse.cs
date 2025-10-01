using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Shared
{
    public class TokenAdminResponse
    {
        public required string AccessToken { get; init; }
        public required long ExpiresIn { get; init; }
        public required string RefreshToken { get; init; }
        public required string Role { get; init; }

    }
}

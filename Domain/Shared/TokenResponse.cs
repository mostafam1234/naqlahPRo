using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Shared
{
    public class TokenResponse
    {
        public TokenResponse()
        {
            this.AccessToken=string.Empty;
            this.RefreshToken=string.Empty;
        }
        public required string AccessToken { get; init; }
        public required long ExpiresIn { get; init; }
        public required string RefreshToken { get; init; }
    }
}

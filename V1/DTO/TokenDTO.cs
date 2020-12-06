using System;

namespace AuthJWT_ASPNETCore2.V1.DTO
{
    public class TokenDTO
    {
        public string Token { get; set; }

        public DateTime Expiration { get; set; }

        public string RefreshToken { get; set; }

        public DateTime ExpirationRefreshToken { get; set; }
    }
}

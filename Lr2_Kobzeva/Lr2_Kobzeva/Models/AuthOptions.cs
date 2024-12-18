using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lr2_Kobzeva
{
    internal class AuthOptions
    {
        public static string Issuer => "EM"; // Издатель токена
        public static string Audience => "APIclients"; // Аудитория токена
        public static int LifetimeInYears => 1; // Время жизни токена
        public static SecurityKey SigningKey => new SymmetricSecurityKey(Encoding.ASCII.GetBytes("superSecretKeyMustBeLoooooong32bitsMore")); // Секретный ключ

        // Метод генерации токена
        internal static object GenerateToken(string username, bool is_admin = false)
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, username),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, is_admin ? "admin" : "user")
            };

            var identity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            var jwt = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                notBefore: now,
                expires: now.AddYears(LifetimeInYears),
                claims: identity.Claims,
                signingCredentials: new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new { token = encodedJwt };
        }
    }
}

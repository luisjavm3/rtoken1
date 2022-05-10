using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using rtoken1.Model;

namespace rtoken1.Utils
{
    public interface IJwtUtils
    {
        Task<RefreshToken> genRToken(User user, string createdById, string firstTokenSession = null);
        string genAccessToken(int userId);
    }

    public class JwtUtils : IJwtUtils
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        public JwtUtils(DataContext context, IConfiguration confiduration)
        {
            _context = context;
            _configuration = confiduration;
        }
        public async Task<RefreshToken> genRToken(User user, string createdById, string firstTokenSession = null)
        {
            var tokenValue = await genRTokenValue();
            var fTokenSession = firstTokenSession == null ? tokenValue : firstTokenSession;

            var token = new RefreshToken
            {
                Value = tokenValue,
                CreatedAt = DateTime.Now,
                CreatedByIp = createdById,
                ExpiresAt = DateTime.Now.AddDays(int.Parse(_configuration.GetSection("AppSettings:RTokenLifetime").Value)),
                FirstSessionToken = fTokenSession,
                User = user
            };

            return token;

            async Task<string> genRTokenValue()
            {
                var value = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                var tokenExists = await _context.RefreshTokens.AnyAsync(t => t.Value.Equals(value));

                if (!tokenExists)
                    return value;

                return await genRTokenValue();
            }
        }

        public string genAccessToken(int userId)
        {
            var encodedKey = System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:AccessTokenKey").Value);
            var signingKey = new SymmetricSecurityKey(encodedKey);
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512Signature);
            var claims = new[] { new Claim("id", userId.ToString()) };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                // Expires = DateTime.Now.AddMinutes(int.Parse(_configuration.GetSection("AppSettings:AccessTokenLifetime").Value)),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration.GetSection("AppSettings:AccessTokenLifetime").Value)),
                SigningCredentials = signingCredentials,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
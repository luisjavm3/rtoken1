using System.Security.Claims;
using System.Security.Cryptography;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using rtoken1.Dtos.Auth;
using rtoken1.Dtos.User;
using rtoken1.Model;
using rtoken1.Utils;

namespace rtoken1.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtUtils _jwtUtils;
        public AuthService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, IJwtUtils jwtUtils)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _jwtUtils = jwtUtils;
        }

        public async Task<ServiceResponse<GetUserDto>> Register(AuthRequest request)
        {
            var response = new ServiceResponse<GetUserDto>();

            try
            {
                var userExists = await _context.Users.AnyAsync(u => u.Username.ToLower().Equals(request.Username));

                if (userExists)
                    throw new Exception("User already exists.");

                PasswordUtils.genPasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = new User
                {
                    Username = request.Username,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                response.Data = _mapper.Map<GetUserDto>(user);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<LoginResponse>> Login(AuthRequest request)
        {
            var response = new ServiceResponse<LoginResponse>();

            try
            {
                var user = await _context.Users
                            .Include(u => u.RTokens)
                            .FirstOrDefaultAsync(u => u.Username.ToLower().Equals(request.Username.ToLower()));

                if (user == null || !PasswordUtils.matchPasswords(request.Password, user.PasswordHash, user.PasswordSalt))
                    throw new Exception("Wrong credentials.");

                // revokes all user's refresh tokens due new login process.
                foreach (var t in user.RTokens)
                {
                    t.RevokedAt ??= DateTime.Now;
                    t.ReasonRevoked ??= "New login process";
                    t.RevokedByIp = getClientIp();

                    // Removes every expired refresh token
                    if (t.IsExpired)
                        _context.RefreshTokens.Remove(t);
                }

                await _context.SaveChangesAsync();

                // Generates a new, unique refresh token and stores it in database.
                var rToken = await _jwtUtils.genRToken(user, getClientIp());
                await _context.RefreshTokens.AddAsync(rToken);
                await _context.SaveChangesAsync();

                // Generates a new, signing JWT access token.
                var accessToken = _jwtUtils.genAccessToken(user.Id);

                var loginResponse = new LoginResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.Role,
                    AccessToken = accessToken,
                    RToken = rToken.Value
                };

                response.Data = loginResponse;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message + ex.StackTrace;
            }

            return response;
        }

        public async Task<ServiceResponse<LoginResponse>> RefreshToken(string cookieRToken)
        {
            var response = new ServiceResponse<LoginResponse>();

            try
            {
                var rToken = await _context.RefreshTokens
                                .Include(t => t.User)
                                .FirstOrDefaultAsync(t => t.Value.Equals(cookieRToken));

                if (rToken == null)
                    throw new Exception("Invalid refresh token.");

                if (rToken.IsExpired)
                {
                    _context.RefreshTokens.Remove(rToken);
                    await _context.SaveChangesAsync();

                    throw new Exception("Refresh token expired.");
                }

                // Revoves all refresh tokens if any revoked refresh token is used to generate a new one.
                if (rToken.IsRevoked)
                {
                    var userRTokens = await _context.RefreshTokens
                                        .Where(t => t.User.Id == rToken.User.Id).ToListAsync();

                    foreach (var t in userRTokens)
                    {
                        t.RevokedAt ??= DateTime.Now;
                        t.ReasonRevoked ??= "Possible identity theft";
                        t.RevokedByIp ??= getClientIp();
                    }

                    await _context.SaveChangesAsync();

                    throw new Exception("Refresh token revoked.");
                }

                // Rotate refresh tokens
                var newRToken = await _jwtUtils.genRToken(rToken.User, getClientIp(), rToken.FirstSessionToken);
                await _context.RefreshTokens.AddAsync(newRToken);
                rToken.RevokedAt = DateTime.Now;
                rToken.ReasonRevoked = "Rotated by new refresh token";
                rToken.RevokedByIp = getClientIp();
                await _context.SaveChangesAsync();

                // Prepares LoginResponse object
                var LoginResponse = new LoginResponse
                {
                    Id = rToken.User.Id,
                    Username = rToken.User.Username,
                    Role = rToken.User.Role,
                    AccessToken = _jwtUtils.genAccessToken(rToken.User.Id),
                    RToken = newRToken.Value
                };

                response.Data = LoginResponse;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<string>> RevokeToken(string cookieRToken)
        {
            var response = new ServiceResponse<string>();

            try
            {
                var rToken = await _context.RefreshTokens
                                .FirstOrDefaultAsync(t => t.Value.Equals(cookieRToken));

                if (rToken == null)
                    throw new Exception("Token does not exist.");

                if (rToken.IsRevoked)
                    throw new Exception("Token already revoked.");

                if (rToken.IsExpired)
                {
                    _context.RefreshTokens.Remove(rToken);
                    await _context.SaveChangesAsync();

                    throw new Exception("Token expired.");
                }

                // Revokes rToken
                rToken.RevokedAt = DateTime.Now;
                rToken.RevokedByIp = getClientIp();
                rToken.ReasonRevoked = "revoked by client at revokeroken route.";
                await _context.SaveChangesAsync();

                response.Data = $"{rToken.Value} revoked";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private string getClientIp()
        {
            bool ipInRequest = _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For");

            return ipInRequest
            ?
                _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"]
            :
            _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
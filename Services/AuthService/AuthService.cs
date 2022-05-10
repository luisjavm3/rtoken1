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
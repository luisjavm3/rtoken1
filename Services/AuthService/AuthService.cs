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
        public AuthService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
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
                            .FirstOrDefaultAsync(u => u.Username.ToLower().Equals(request.Username.ToLower()));

                if (user == null || !PasswordUtils.matchPasswords(request.Password, user.PasswordHash, user.PasswordSalt))
                    throw new Exception("Wrong credentials.");
                



            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

    }
}
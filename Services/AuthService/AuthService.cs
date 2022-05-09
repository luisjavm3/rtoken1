using rtoken1.Dtos.Auth;
using rtoken1.Dtos.User;
using rtoken1.Model;

namespace rtoken1.Services.AuthService
{
    public class AuthService : IAuthService
    {
        public Task<ServiceResponse<GetUserDto>> Register(AuthRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
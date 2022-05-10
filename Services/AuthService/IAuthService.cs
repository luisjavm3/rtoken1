using rtoken1.Dtos.Auth;
using rtoken1.Dtos.User;
using rtoken1.Model;

namespace rtoken1.Services.AuthService
{
    public interface IAuthService
    {
        Task<ServiceResponse<GetUserDto>> Register(AuthRequest request);
        Task<ServiceResponse<LoginResponse>> Login(AuthRequest request);
        Task<ServiceResponse<LoginResponse>> RefreshToken(string rToken);
    }
}
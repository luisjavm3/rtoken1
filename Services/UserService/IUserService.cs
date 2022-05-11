using rtoken1.Dtos.User;
using rtoken1.Model;

namespace rtoken1.Services.UserService
{
    public interface IUserService
    {
        Task<ServiceResponse<GetUserDto>> GetById(int id);
        Task<ServiceResponse<List<GetUserDto>>> All();
    }
}
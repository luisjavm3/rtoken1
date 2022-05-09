using AutoMapper;
using rtoken1.Dtos.User;
using rtoken1.Model;

namespace rtoken1
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, GetUserDto>();
        }
    }
}
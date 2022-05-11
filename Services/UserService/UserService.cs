using AutoMapper;
using Microsoft.EntityFrameworkCore;
using rtoken1.Dtos.User;
using rtoken1.Model;
using rtoken1.Utils;

namespace rtoken1.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<GetUserDto>> GetById(int id)
        {
            var response = new ServiceResponse<GetUserDto>();

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    throw new Exception("User not found.");

                response.Data = _mapper.Map<GetUserDto>(user);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<List<GetUserDto>>> All()
        {
            var response = new ServiceResponse<List<GetUserDto>>();

            try
            {
                var allUsers = await _context.Users.ToListAsync();

                response.Data = allUsers.Select(u => _mapper.Map<GetUserDto>(u)).ToList();
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
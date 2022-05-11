using Microsoft.AspNetCore.Mvc;
using rtoken1.Dtos.User;
using rtoken1.Model;
using rtoken1.Services.UserService;

namespace rtoken1.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> GetById(int id)
        {
            var response = await _userService.GetById(id);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> All()
        {
            var response = await _userService.All();

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

    }
}
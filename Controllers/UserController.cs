using Microsoft.AspNetCore.Mvc;
using rtoken1.Authorization;
using rtoken1.Dtos.User;
using rtoken1.Model;
using rtoken1.Services.UserService;

namespace rtoken1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _contextAccessor;

        public UserController(IUserService userService, IHttpContextAccessor contextAccessor)
        {
            _userService = userService;
            _contextAccessor = contextAccessor;
        }

        [Authorize(Role.Super, Role.Admin)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> GetById(int id)
        {
            var response = await _userService.GetById(id);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [Authorize(Role.Super, Role.Admin)]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> All()
        {
            var response = await _userService.All();

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("Greetings")]
        public string Greetings()
        {
            var user = (GetUserDto)_contextAccessor.HttpContext.Items["User"];
            return $"Hello {user.Username}. Hope you are doing great!";
        }

    }
}
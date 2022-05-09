using Microsoft.AspNetCore.Mvc;
using rtoken1.Dtos.Auth;
using rtoken1.Dtos.User;
using rtoken1.Model;
using rtoken1.Services.AuthService;

namespace rtoken1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> Register(AuthRequest request)
        {
            var response = await _authService.Register(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
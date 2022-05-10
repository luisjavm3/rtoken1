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
        private readonly IConfiguration _configuration;
        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> Register(AuthRequest request)
        {
            var response = await _authService.Register(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ServiceResponse<LoginResponse>>> Login(AuthRequest request)
        {
            var response = await _authService.Login(request);

            if (!response.Success)
                return BadRequest(response);

            setRTokenCookie(response.Data.RToken);

            return Ok(response);
        }

        private void setRTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddDays(int.Parse(_configuration.GetSection("AppSettings:RTokenLifetime").Value))
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using rtoken1.Dtos.Auth;
using rtoken1.Dtos.User;
using rtoken1.Model;

namespace rtoken1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost]
        public Task<ActionResult<ServiceResponse<GetUserDto>>> Register(AuthRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
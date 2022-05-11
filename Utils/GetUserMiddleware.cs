using System.Security.Claims;
using rtoken1.Dtos.User;
using rtoken1.Services.UserService;

namespace rtoken1.Utils
{
    public class GetUserMiddleware
    {
        private RequestDelegate _next;
        public GetUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            try
            {
                int userId = int.Parse(context.User.FindFirstValue(claimType: "id"));

                var userResponse = await userService.GetById(userId);
                context.Items["User"] = (GetUserDto)userResponse.Data;
            }
            catch (System.Exception)
            {
                context.Items["User"] = null;
            }

            await _next(context);
        }
    }
}
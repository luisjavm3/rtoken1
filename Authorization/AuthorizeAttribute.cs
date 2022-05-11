using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using rtoken1.Dtos.User;
using rtoken1.Model;

namespace rtoken1.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class Authorize : Attribute, IAuthorizationFilter
    {
        private readonly IList<Role> _roles;

        public Authorize(IList<Role> roles)
        {
            _roles = roles ?? new Role[] { };
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();

            if (allowAnonymous)
                return;

            var user = (GetUserDto)context.HttpContext.Items["User"];

            if (user == null || (_roles.Any() && !_roles.Contains(user.Role)))
            {
                context.Result = new JsonResult(new { data = "", success = false, message = "Unauthorized" })
                { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
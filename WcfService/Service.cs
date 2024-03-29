using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WcfService
{
    [Authorize]
    public partial class Service : IService
    {
        [Authorize(Policy = "is-teacher")]
        public string GetData(int value, [FromServices] HttpContext context)
        {
            var name = context.User.FindFirstValue("name");

            return $"Hello, {name}. You entered: {value}";
        }
    }
}

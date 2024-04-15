using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WcfService
{
    [Authorize]
    public partial class Service : IService
    {
        public string GetData(int value, [FromServices] HttpContext context)
        {
            var name = context.User.Identity!.Name;

            return $"Hello, {name}. You entered: {value}";
        }
    }
}

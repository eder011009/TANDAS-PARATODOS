using System.Security.Claims;
using System.Threading.Tasks;
using AngelesTandas.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace AngelesTandas.Web.Middleware
{
    public class CurrentUserMiddleware
    {
        private readonly RequestDelegate _next;

        public CurrentUserMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            var userId = context.User?.Identity?.IsAuthenticated == true
                ? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            db.SetCurrentUser(userId);

            try
            {
                await _next(context);
            }
            finally
            {
                db.SetCurrentUser(null);
            }
        }
    }
}
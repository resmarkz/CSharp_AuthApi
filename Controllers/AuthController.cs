using AuthApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthAPIContext _context;
        public AuthController(AuthAPIContext context)
        {
            _context = context;
        }
    }
}

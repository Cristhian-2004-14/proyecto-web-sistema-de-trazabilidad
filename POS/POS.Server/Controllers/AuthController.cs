using Microsoft.AspNetCore.Mvc;
using POS.Server.Services;
using POS.Shared.DTOs;

namespace POS.Server.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await auth.LoginAsync(request);
        return result is null ? Unauthorized(new { message = "Usuario o contraseña incorrectos." }) : Ok(result);
    }
}

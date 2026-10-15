using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;

namespace Cross_Ruseva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Метод для получения токена (по умолчанию выдает роль User)
        [HttpGet("get-token")]
        public IActionResult GetToken([FromQuery] string role = "User")
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKeyForMyCrossPlatformProject2026!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: System.DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
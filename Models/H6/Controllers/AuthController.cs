using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestAcessTonken.Models;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace TestAcessTonken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
       
        private readonly IConfiguration _configuration;

        public List<User> ReadUser()
        {
            var json = System.IO.File.ReadAllText("users.json");
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginModel login)
        {
            var list = ReadUser();
            var user = list.FirstOrDefault(u => u.UserName == login.UserName && u.Password == login.Password);
            if(user == null)
            {
                return Unauthorized("invalid");
            }
            var jwtsetting = _configuration.GetSection("JwtSettings");
            var accesstoken = GenerateJwtToken(user.UserName, jwtsetting["SecretKey"], jwtsetting["Issuer"], jwtsetting["Audience"], TimeSpan.FromMinutes(1));
            return Ok(new {accesstoken});
        }

        private string GenerateJwtToken(string username, string secret, string issuer, string audience, TimeSpan expiry)
        {
             var claims = new[]
             {
                  new Claim(ClaimTypes.Name, username),
                  new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(expiry),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }





    }
}

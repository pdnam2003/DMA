using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TOKEN6.Models;
using System.Text.Json;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace TOKEN6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginModel userLogin)
        {
            var users = readUser();
            var user = users.FirstOrDefault(u=>u.UserName == userLogin.UserName && u.Password == userLogin.Password );
            if (user == null)  return Unauthorized("thong tin dang nhap sai");

            var jwtsetting = _configuration.GetSection("JwtSettings");

            var accesstoken = GenerateJwtToken(user.UserName, jwtsetting["SecretKey"], jwtsetting["Issuer"], jwtsetting["Audience"], TimeSpan.FromSeconds(15));
            var refreshToken = Guid.NewGuid().ToString();
            
            user.RefreshToken = refreshToken;
            WriteUser(users);

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            { 
                HttpOnly = true ,
                Secure = true ,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(10),
                Path = "/"
            }
            
            );


            return Ok(new { accesstoken });
            
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

        [Authorize]
        [HttpGet("data")]
        public IActionResult Getdata()
        {
            return Ok(new { message = "du lieu bi mat, 0 1 ai dc truy cap " });
        }

        private List<User> readUser()
        {
            var json  = System.IO.File.ReadAllText("users.json");

            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public void WriteUser(List<User> u)
        {
            var json = JsonSerializer.Serialize(u);
            System.IO.File.WriteAllText("users.json", json);
        }
    }
}

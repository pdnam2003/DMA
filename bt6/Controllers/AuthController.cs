using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Token6.Models;

namespace Token6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginModel userLogin)
        {
            var users = readUser();
            var user = users.FirstOrDefault(u=> u.Username == userLogin.Username && u.Password == userLogin.Password);
            if (user == null)
            {
                return Unauthorized("sai thong tin dang nhap");
            }
            var jwtSetting = _configuration.GetSection("JwtSettings");

            var accessToken = GenerateJwtToken(
                user.Username,
                jwtSetting["SecretKey"],
                jwtSetting["Issuer"],
                jwtSetting["Audience"],
                TimeSpan.FromMinutes(1)
                ); 
            var  refreshToken = Guid.NewGuid().ToString();

            user.Refresh = refreshToken;
            WriteUser(users);

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddYears(1),
                Path = "/"
            });
            return Ok(new { accessToken });
        }
        [Authorize]
        [HttpGet("data")]
        public IActionResult GetData()
        {
         
            return Ok(new { message = "du lieu an" });
        }



        private List<User> readUser()
        {
            var json = System.IO.File.ReadAllText("users.json");
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
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

        public void WriteUser(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText("users.json", json);
        }
        [HttpPost("refresh")]
        public IActionResult Refresh()
        {
            // 1) Lấy refresh token từ Cookie HttpOnly
            var refresh = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refresh))
                return Unauthorized("Thiếu refresh token trong cookie.");

            // 2) Tìm user có refresh token này
            var users = readUser(); 
            var user = users.FirstOrDefault(u => u.Refresh == refresh);
            if (user == null)
                return Unauthorized("Refresh token không hợp lệ.");

            // 3) Phát hành access token mới
            var jwtSetting = _configuration.GetSection("JwtSettings");
            var access = GenerateJwtToken(
                user.Username,
                jwtSetting["SecretKey"],
                jwtSetting["Issuer"],
                jwtSetting["Audience"],
                TimeSpan.FromMinutes(1)
            );

            // (tuỳ chọn) rotation: đổi refresh token mới để an toàn hơn
            // var newRefresh = Guid.NewGuid().ToString();
            // user.Refresh = newRefresh;
            // WriteUser(users);
            // Response.Cookies.Append("refreshToken", newRefresh, new CookieOptions {
            //     HttpOnly = true,
            //     Secure = true,                // Dev HTTP thì cân nhắc false
            //     SameSite = SameSiteMode.None, // cross-site
            //     Expires = DateTime.UtcNow.AddYears(1),
            //     Path = "/"
            // });

            return Ok(new { accessToken = access });
        }


    }

}

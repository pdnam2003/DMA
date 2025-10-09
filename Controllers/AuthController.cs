using LoginApi.DTOs;
using LoginApi.Models;
using LoginApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LoginApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserStore _Store;
        public AuthController(IUserStore store)
        {
            _Store = store;
        }
        [HttpPost("register")]
        public  async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if(string.IsNullOrWhiteSpace(req.Username) 
                || string.IsNullOrWhiteSpace(req.Password) 
                || string.IsNullOrEmpty(req.Phone))
            {
                return BadRequest(new {message = "thieu thong tin de dang nhap"});
            }

            if(!req.Phone.StartsWith("+84") && !req.Phone.StartsWith("0"))
            {
                return BadRequest(new {message = "so dien thoai phai bat dau 0 hoac +84"});

            }
            var ok = await _Store.AddUserAsync(new User
            {
                Username = req.Username.Trim(),
                Password = req.Password.Trim(),
                Phone = req.Phone.Trim(),
                Verified = false

            });

            if(!ok)
            {
                return Conflict(new {message = "tai khoan da ton tai"});
            }
            return Ok(new {message = "dang ky thanh cong, ban co the dang nhap"});
            }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if(string.IsNullOrWhiteSpace(req.Username) 
                || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new {message = "thieu thong tin de dang nhap"});
            }

            var user = await _Store.FindByUsernameAsync(req.Username);

            if(user == null || user.Password != req.Password)
            {
                return Unauthorized(new {message = "tai khoan hoac mat khau khong dung"});
            }
            var resp = new LoginResponse
            {
                Username = user.Username,
                Phone = user.Phone,
                Verified = user.Verified,
                Message = "dang nhap thanh cong"
            };

          return Ok(resp);
        }














    }
}

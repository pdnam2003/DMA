namespace LoginApi.DTOs
{
    public class LoginResponse
    {
        public string Username { get; set; } = "";
        public string Phone { get; set; } = "";
        public bool Verified { get; set; } = false;
        public string Message { get; set; } = "Đăng nhập thành công";
    }
}

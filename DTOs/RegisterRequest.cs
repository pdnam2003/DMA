﻿namespace LoginApi.DTOs
{
    public class RegisterRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Phone { get; set; } = "";
    }
}

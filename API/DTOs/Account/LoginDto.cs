﻿using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Account
{
    public class LoginDto
    {
        [Required(ErrorMessage ="UserName is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}

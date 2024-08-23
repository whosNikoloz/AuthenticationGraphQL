﻿using System.ComponentModel.DataAnnotations;

namespace AuthenticationGraphQL.Dto.LoginRequest
{
    public class UserLoginEmailRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

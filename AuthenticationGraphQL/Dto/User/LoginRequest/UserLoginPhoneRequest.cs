using System.ComponentModel.DataAnnotations;

namespace AuthenticationGraphQL.Dto.User.LoginRequest
{
    public class UserLoginPhoneRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;


        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

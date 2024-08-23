using System.ComponentModel.DataAnnotations;

namespace AuthenticationGraphQL.Dto
{
    public class CheckEmailRequest
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

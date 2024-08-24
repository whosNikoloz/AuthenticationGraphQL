using System.ComponentModel.DataAnnotations;

namespace AuthenticationGraphQL.Models
{
    public class UserModel
    {

        [Key]
        public int UserId { get; set; }

        [StringLength(50, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 50 characters.")]
        public string? UserName { get; set; }

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Picture { get; set; }

        public byte[] PasswordHash { get; set; } = new byte[32];

        public byte[] PasswordSalt { get; set; } = new byte[32];

        public string? VerificationToken { get; set; }

        public DateTime VerifiedAt { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; }

        public string? Role { get; set; }

        // OAuth-specific properties
        public string? OAuthProvider { get; set; }
        public string? OAuthProviderId { get; set; }

        public DateTime LastActivity { get; set; }

        // Navigation properties for following/followers
        public ICollection<UserFollowModel> Followers { get; set; } = new List<UserFollowModel>();
        public ICollection<UserFollowModel> Following { get; set; } = new List<UserFollowModel>();

    }
}

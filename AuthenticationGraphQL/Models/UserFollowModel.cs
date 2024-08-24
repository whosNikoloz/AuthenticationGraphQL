using System.ComponentModel.DataAnnotations;

namespace AuthenticationGraphQL.Models
{
    public class UserFollowModel
    {
        [Key]
        public int Id { get; set; }

        public int FollowerId { get; set; }
        public UserModel Follower { get; set; } = null!;

        public int FollowingId { get; set; }
        public UserModel Following { get; set; } = null!;
    }
}

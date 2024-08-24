using AuthenticationGraphQL.Dto;
using AuthenticationGraphQL.Models;

namespace AuthenticationGraphQL.Services
{
    public interface IUserFollowService
    {
        Task<bool> FollowUserAsync(int followerId, int followingId);
        Task<bool> UnfollowUserAsync(int followerId, int followingId);
        Task<bool> IsFollowingAsync(int followerId, int followingId);

        Task<List<FollowUserDto>> GetFollowersAsync(int userId);

        Task<List<FollowUserDto>> GetFollowingAsync(int userId);

    }
}

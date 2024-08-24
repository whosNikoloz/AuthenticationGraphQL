using AuthenticationGraphQL.Services;

namespace AuthenticationGraphQL.GraphQL
{
    public class Mutation
    {
        public string SayHello(string name) => $"Hello, {name}!";

        private readonly IUserFollowService _userFollowService;

        public Mutation(IUserFollowService userFollowService)
        {
            _userFollowService = userFollowService;
        }

        public async Task<bool> FollowUserAsync(int followerId, int followingId)
        {
            return await _userFollowService.FollowUserAsync(followerId, followingId);
        }

        public async Task<bool> UnfollowUserAsync(int followerId, int followingId)
        {
            return await _userFollowService.UnfollowUserAsync(followerId, followingId);
        }

    }
}

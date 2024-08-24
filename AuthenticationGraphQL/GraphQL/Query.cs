using AuthenticationGraphQL.Dto;
using AuthenticationGraphQL.GraphQL.Types;
using AuthenticationGraphQL.Models;
using AuthenticationGraphQL.Services;
using HotChocolate.Authorization;


namespace AuthenticationGraphQL.GraphQL
{
    public class Query
    {
        private readonly IUserService _userService;
        private readonly IUserFollowService _userFollowService;

        public Query(IUserService userService, IUserFollowService userFollowService)
        {
            _userService = userService;
            _userFollowService = userFollowService;
        }

        public string Hello()
        {
            return "Hello";
        }

        [Authorize(Roles = new[] { "admin" })]
        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            return await _userService.GetUsersAsync();
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followingId)
        {
            return await _userFollowService.IsFollowingAsync(followerId, followingId);
        }

    }
}

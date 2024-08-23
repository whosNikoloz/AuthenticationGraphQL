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
        public Query(IUserService userService)
        {
            _userService = userService;
        }
        public string Hello()
        {
            return "Heelloo";
        }

        [Authorize(Roles = new[] { "admin" })]
        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            return await _userService.GetUsersAsync();
        }

    }
}

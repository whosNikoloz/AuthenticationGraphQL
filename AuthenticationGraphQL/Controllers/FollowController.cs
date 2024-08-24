using AuthenticationGraphQL.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationGraphQL.Controllers
{
    public class FollowController : Controller
    {
        private readonly IUserFollowService _userFollowService;

        public FollowController(IUserFollowService userFollowService)
        {
            _userFollowService = userFollowService;
        }

        [HttpPost("follow")]
        public async Task<IActionResult> FollowUser(int followerId, int followingId)
        {
            var result = await _userFollowService.FollowUserAsync(followerId, followingId);
            if (result)
            {
                return Ok(new { message = "Successfully followed user." });
            }
            return BadRequest(new { message = "Failed to follow user or already following." });
        }

        [HttpPost("unfollow")]
        public async Task<IActionResult> UnfollowUser(int followerId, int followingId)
        {
            var result = await _userFollowService.UnfollowUserAsync(followerId, followingId);
            if (result)
            {
                return Ok(new { message = "Successfully unfollowed user." });
            }
            return BadRequest(new { message = "Failed to unfollow user or not following." });
        }

        [HttpGet("isfollowing")]
        public async Task<IActionResult> IsFollowing(int followerId, int followingId)
        {
            var result = await _userFollowService.IsFollowingAsync(followerId, followingId);
            return Ok(new { isFollowing = result });
        }
    }
}

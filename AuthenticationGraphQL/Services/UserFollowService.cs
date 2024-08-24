using AuthenticationGraphQL.Data;
using AuthenticationGraphQL.Dto;
using AuthenticationGraphQL.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationGraphQL.Services
{
    public class UserFollowService : IUserFollowService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public UserFollowService(AppDbContext db,IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<bool> FollowUserAsync(int followerId, int followingId)
        {
            if (followerId == followingId)
            {
                // A user cannot follow themselves
                return false;
            }

            var existingFollow = await _db.UserFollows
                .AnyAsync(uf => uf.FollowerId == followerId && uf.FollowingId == followingId);

            if (existingFollow)
            {
                // Already following
                return false;
            }

            var userFollow = new UserFollowModel
            {
                FollowerId = followerId,
                FollowingId = followingId
            };

            _db.UserFollows.Add(userFollow);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowUserAsync(int followerId, int followingId)
        {
            var userFollow = await _db.UserFollows
                .FirstOrDefaultAsync(uf => uf.FollowerId == followerId && uf.FollowingId == followingId);

            if (userFollow == null)
            {
                // Not following
                return false;
            }

            _db.UserFollows.Remove(userFollow);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followingId)
        {
            return await _db.UserFollows
                .AnyAsync(uf => uf.FollowerId == followerId && uf.FollowingId == followingId);
        }

        public async Task<List<FollowUserDto>> GetFollowersAsync(int userId)
        {
            var FollowerUsers = await _db.UserFollows
                .Where(uf => uf.FollowingId == userId)
                .Join(_db.Users,
                      uf => uf.FollowerId,
                      u => u.UserId,
                      (uf, u) => new FollowUserDto
                      {
                          UserId = u.UserId,
                          Email = u.Email,
                          UserName = u.UserName,
                          FirstName = u.FirstName,
                          LastName = u.LastName,
                      })
                .ToListAsync();
            return FollowerUsers;
        }

        public async Task<List<FollowUserDto>> GetFollowingAsync(int userId)
        {
            var FollowerUsers = await _db.UserFollows
                .Where(uf => uf.FollowerId == userId)
                .Join(_db.Users,
                      uf => uf.FollowingId,
                      u => u.UserId,
                      (uf, u) => new FollowUserDto
                      {
                          UserId = u.UserId,
                          Email = u.Email,
                          UserName = u.UserName,
                          FirstName = u.FirstName,
                          LastName = u.LastName,
                      })
                .ToListAsync();
            return FollowerUsers;
        }
    }
}

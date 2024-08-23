﻿using AuthenticationGraphQL.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthenticationGraphQL.Services
{
    public class StaticFuncs : IStatiFuncs
    {

        private readonly IConfiguration _configuration;
        public StaticFuncs(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        public string CreateToken(UserModel user)
        {
            List<Claim> claims;
            try
            {
                claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, (user.Email != null ? user.Email : "")),
                new Claim(ClaimTypes.Name, (user.FirstName != null ? user.FirstName : "")),
                new Claim(ClaimTypes.Surname, (user.LastName != null ? user.LastName : "")),
                new Claim(ClaimTypes.NameIdentifier, (user.UserName != null ? user.UserName : "")),
                new Claim(ClaimTypes.MobilePhone, (user.PhoneNumber != null ? user.PhoneNumber : "") ),
                new Claim("ProfilePicture", (user.Picture != null ? user.Picture : "")),
                new Claim("joinedAt", user.VerifiedAt.ToString()),
                new Claim("Oauth", (user.OAuthProvider == null ? "" : user.OAuthProvider)),
                new Claim(ClaimTypes.Role, (user.Role != null ? user.Role : "")),
            };
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred while creating claims: {ex.Message}");
                // You can choose to throw the exception further if it's not recoverable
                throw;
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public (string, string) ExtractNamesFromUsername(string username)
        {
            // Split the username into parts based on spaces
            var nameParts = username.Split(' ');

            // Take the first part as the first name
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";

            // Take the rest as the last name
            var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

            return (firstName, lastName);
        }


        
    }
}

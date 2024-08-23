using AuthenticationGraphQL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using AuthenticationGraphQL.Data;
using Microsoft.AspNetCore.Authorization;
using AuthenticationGraphQL.Services;
using AuthenticationGraphQL.Dto;
using AuthenticationGraphQL.Dto.LoginRequest;
using AuthenticationGraphQL.Dto.Password;

namespace AuthenticationGraphQL.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public UserController(IEmailService emailService, IUserService userService)
        {
            _userService = userService;
            _emailService = emailService;
        }


        [HttpGet("Test")]
        public IActionResult Test()
        {
            return Ok(_userService.Test());
        }

        // მოიძიეთ ყველა მომხმარებლის სია.
        // მოითხოვს ადმინისტრატორის პრივილეგიებს.
        // GET api/Users
        [HttpGet("Users"), Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            if(users == null)
            {
                return NotFound("Users not found");
            }
            return Ok(users);
        }

        // მიიღეთ კონკრეტული მომხმარებლის პროფილი მომხმარებლის სახელით.
        // საჭიროებს ავთენტიფიკაციას.
        // GET api/User/{username}
        [HttpGet("User/{userid}"), Authorize]
        public async Task<IActionResult> GetUser(int userid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Call the service method to get the user
            var response = await _userService.GetUserAsync(userid);

            // Check if there was an error
            if (response.Error != null)
            {
                return NotFound(response.Error); // Return 404 if user was not found
            }

            // Return the user data and token if everything is fine
            return Ok(new
            {
                User = response.User,
                Token = response.Token
            });
        }


        [HttpDelete("User/{userid}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int userid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.DeleteUserAsync(userid);

            if (result is NotFoundObjectResult notFoundResult)
            {
                var message = notFoundResult.Value as string;

                return NotFound(message);
            }

            return result;
        }



        [HttpPost("Auth/Login/check-email")]
        public async Task<IActionResult> CheckEmailLogin(CheckEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var response = await _userService.CheckEmailLoginAsync(request);

            if (!response)
            {
                return Ok(new
                {
                    successful = false,
                    error = "ასეთი მეილი არარსებობს"
                });
            }

            return Ok(new
            {
                Successful = true
            });
        }

        [HttpPost("Auth/Register/check-email")]
        public async Task<IActionResult> CheckEmailReg(CheckEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userService.CheckEmailRegistrationAsync(request);

            if (response)
            {
                return Ok(new
                {
                    successful = false,
                    error = "ასეთი მეილი უკვე არსებობს"
                });
            }

            return Ok(new
            {
                Successful = true
            });
        }

        [HttpGet("Auth/Register/check-username/{username}")]
        public async Task<IActionResult> CheckUserName(string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var response = await _userService.CheckUserNameAsync(username);

            if (response)
            {
                return Ok(new
                {
                    successful = false,
                    error = "სახელი დაკავებულია"
                });
            }

            return Ok(new
            {
                Successful = true
            });
        }

        // ახალი მომხმარებლის რეგისტრაცია.
        // POST api/Auth/რეგისტრაცია
        [HttpPost("Auth/Register")]
        public async Task<IActionResult> RegisterUser(UserRegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ResponseUser response = await _userService.RegisterUserAsync(request);

            if(response.Error != null)
            {
                return BadRequest(response.Error);
            }


            string host = "localhost:7213";

            string verificationLink = Url.ActionLink("VerifyEmail", "User", new { token = response.User.VerificationToken }, Request.Scheme, host);


            await _emailService.SendVerificationEmailAsync(response.User.Email, response.User.UserName, verificationLink);

            return Ok("User successfully created. Verification email sent.");
        }


        [HttpPost("Auth/RegisterOAuth2")]
        public async Task<IActionResult> RegisterOAuthUser(OAuthUserRegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userService.RegisterOAuthUserAsync(request);

            if (response is BadRequestObjectResult badRequestResult)
            {
                var message = badRequestResult.Value as string;

                return BadRequest(message);
            }
            return Ok(response);
        }

        // ამოიღეთ მომხმარებელი ID-ით.
        // მოითხოვს ადმინისტრატორის პრივილეგიებს.
        // DELETE api/Auth/Remove/{userid}
        [HttpDelete("Auth/Remove/{userid}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> RemoveUser(int userid)
        {
            var repsonse = await _userService.RemoveUserAsync(userid);

            if (repsonse is NotFoundObjectResult notFoundResult)
            {
                var message = notFoundResult.Value as string;

                return NotFound(message);
            }else if (repsonse is StatusCodeResult)
            {
                var message = "Server 500 While Saving";
                return StatusCode(500, message);
            }
            return repsonse; 
        }


        [HttpPost("Auth/OAuthEmail")]
        public async Task<IActionResult> LoginOAuth2(OAuth2LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ResponseUser response = await _userService.LoginOAuth2Async(request);

            if(response == null)
            {
                return BadRequest("User not found");
            }

            return Ok(new
            {
                User = response.User,
                Token = response.Token
            });
        }



        // შეამოწმებს თუ Oauth2 მომხმარებელი არსებობს.
        // POST api/Auth/OAuth2Exist
        [HttpPost("Auth/OAuth2Exist")]
        public async Task<IActionResult> CheckOAuth2Exist(CheckOauth2ExistsReqeust reqeust)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _userService.CheckOAuth2ExistAsync(reqeust);
            return Ok(response);
        }

        // შედით ელექტრონული ფოსტით და პაროლით.
        // POST api/Auth/Email
        [HttpPost("Auth/Email")]
        public async Task<IActionResult> LoginWithEmail(UserLoginEmailRequest request)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ResponseUser response = await _userService.LoginWithEmailAsync(request);
            if(response.Error != null)
            {
                return BadRequest(response.Error);
            }
            return Ok(new
            {
                User = response.User,
                Token = response.Token
            });
        }

        // შედით მომხმარებლის სახელით და პაროლით.
        // POST api/Auth/Username
        [HttpPost("Auth/Username")]
        public async Task<IActionResult> LoginWithUserName(UserLoginUserNameRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ResponseUser response = await _userService.LoginWithUserNameAsync(request);
            if (response.Error != null)
            {
                return BadRequest(response.Error);
            }
            return Ok(new
            {
                User = response.User,
                Token = response.Token
            });

        }

        // შედით ტელეფონის ნომრით და პაროლით.
        // POST api/Auth/Phone
        [HttpPost("Auth/Phone")]
        public async Task<IActionResult> LoginWithPhoneNumber(UserLoginPhoneRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ResponseUser response = await _userService.LoginWithPhoneNumberAsync(request);
            if (response.Error != null)
            {
                return BadRequest(response.Error);
            }
            return Ok(new
            {
                User = response.User,
                Token = response.Token
            });
        }


        // შეცვალეთ მომხმარებლის პაროლი.
        // საჭიროებს ავთენტიფიკაციას.
        // POST api/User/ChangeGeneral
        [HttpPut("User/ChangeGeneral"), Authorize]
        public async Task<IActionResult> ChangeGeneral(ChangeGeneralRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var response = await _userService.ChangeGeneralInfoAsync(request, Int32.Parse(userId), JWTRole);

            if(response is NotFoundObjectResult notFoundResult)
            {
                var message = notFoundResult.Value as string;

                return NotFound(message);
            }
            else if (response is StatusCodeResult)
            {
                var message = "Server 500 While Saving";
                return StatusCode(500, message);
            }
            return response;
        }



        // შეცვალეთ მომხმარებლის პაროლი.
        // საჭიროებს ავთენტიფიკაციას.
        // POST api/User/ChangePassword
        [HttpPut("User/ChangePassword"), Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var response = await _userService.ChangePasswordAsync(request, Int32.Parse(userId), JWTRole);

            if (response is NotFoundObjectResult notFoundResult)
            {
                var message = notFoundResult.Value as string;

                return NotFound(message);
            }
            else if (response is StatusCodeResult)
            {
                var message = "Server 500 While Saving";
                return StatusCode(500, message);
            }
            return response;
        }


        // შეცვალეთ მომხმარებლის სახელი ან ტელეფონის ნომერი.
        // საჭიროებს ავთენტიფიკაციას.
        // POST api/User/ChangeUsernameOrNumber
        [HttpPost("User/ChangeUsernameOrNumber"), Authorize]
        public async Task<IActionResult> ChangeUsernameOrPhoneNumber(UserModel requestuser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            var response = await _userService.ChangeUsernameOrPhoneNumberAsync(requestuser , Int32.Parse(userId), JWTRole);

            if(response is NotFoundObjectResult notFoundResult)
            {
                var message = notFoundResult.Value as string;

                return NotFound(message);
            }
            else if (response is StatusCodeResult)
            {
                var message = "Server 500 While Saving";
                return StatusCode(500, message);
            }
            return response;
        }


        // ატვირთეთ მომხმარებლის პროფილის სურათი.
        // საჭიროებს ავთენტიფიკაციას.
        // POST api/User/UploadImage
        [HttpPost("User/UploadImage"), Authorize]
        public async Task<IActionResult> UploadUserProfileImage(UploadImageRequest imagerequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role


            var response = await _userService.UploadUserProfileImageAsync(imagerequest, userId, JWTRole);

            if (response is NotFoundObjectResult notFoundResult)
            {
                var message = notFoundResult.Value as string;

                return NotFound(message);
            }
            else if (response is StatusCodeResult)
            {
                var message = "Server 500 While Saving";
                return StatusCode(500, message);
            }
            return response;
        }



        // გადაამოწმეთ მომხმარებლის ელ.ფოსტის მისამართი ნიშნის გამოყენებით.
        // GET api/Verify/Email
        [HttpGet("Verify/Email")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userService.VerifyEmailAsync(token);

            if (!response)
            {
                return BadRequest("Verification Failed");
            }

            string verificationSuccessUrl = "https://edu-space.vercel.app/en/user/auth/verification-successful";

            // Redirect the user to the verification success URL
            return Redirect(verificationSuccessUrl);
        }



        // მოითხოვეთ პაროლის აღდგენა ელექტრონული ფოსტით.
        // POST api/User/ForgotPass
        [HttpPost("User/ForgotPassword")]
        public async Task<IActionResult> ForgotPasswordRequest(string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userService.ForgotPasswordRequestAsync(email);
            if(response is NotFoundObjectResult notFoundResult)
            {
                var message = notFoundResult.Value as string;

                return NotFound(message);
            }
            else if (response is StatusCodeResult)
            {
                var message = "Server 500 While Saving";
                return StatusCode(500, message);
            }
            return response;
        }



        // გადააყენეთ მომხმარებლის პაროლი გადატვირთვის ნიშნის გამოყენებით.
        // POST api/User/ResetPassword
        [HttpPut("User/ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _userService.ResetPasswordAsync(request);  

            if (response is NotFoundObjectResult notFoundResult)
            {
                var message = notFoundResult.Value as string;

                return NotFound(message);
            }
            else if (response is StatusCodeResult)
            {
                var message = "Server 500 While Saving";
                return StatusCode(500, message);
            }
            return response;
        }



        [HttpPost("User/ChangeEmailRequest/{email}"), Authorize]
        public async Task<IActionResult> ChangeEmailRequest(string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var response = await _userService.ChangeEmailRequestAsync(email, Int32.Parse(userId));
            if(response is NotFoundObjectResult notFoundResult)
            {
                var message = notFoundResult.Value as string;

                return NotFound(message);
            }
            else if (response is StatusCodeResult)
            {
                var message = "Server 500 While Saving";
                return StatusCode(500, message);
            }
            return response;
        }



        [HttpPost("User/ChangeEmail/{email}"), Authorize]
        public async Task<IActionResult> ChangeEmail(string email)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var response = await _userService.ChangeEmailAsync(email, Int32.Parse(userId));
            if (response is NotFoundObjectResult notFoundResult)
            {
                var message = notFoundResult.Value as string;

                return NotFound(message);
            }
            else if (response is StatusCodeResult)
            {
                var message = "Server 500 While Saving";
                return StatusCode(500, message);
            }
            return response;
        }


        [HttpGet("User/ReLogin/{password}"), Authorize]
        public async Task<IActionResult> ReLogin(string password)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var response = await _userService.ReLoginAsync(password, userId);
            if (response is NotFoundObjectResult notFoundResult)
            {
                var message = notFoundResult.Value as string;

                return NotFound(message);
            }
            else if (response is StatusCodeResult)
            {
                var message = "Server 500 While Saving";
                return StatusCode(500, message);
            }
            return response;
        }

    }
}

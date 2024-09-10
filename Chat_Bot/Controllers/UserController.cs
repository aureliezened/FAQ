using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Business;
using Common.DTOs.Request;
using Common.DTOs.Response;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Common.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Common.Models;
using Microsoft.AspNetCore.Identity.Data;


namespace Chat_Bot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("Login")]
        public async Task<ApiResponseType<UserResponse?>> Login([FromBody] LoginReq request)
        {
            _logger.LogInformation($"{nameof(Login)}: UserController.");

            try
            {
                var data = await _userService.AuthenticateUserAsync(request);
                if (data == null)
                {
                    var errorResponse = StatusCodeHelper.GetStatusResponse(401, data);
                    return errorResponse;
                }
                var response = StatusCodeHelper.GetStatusResponse(200, data);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Login)}: UserController.");
                var errorResponse = StatusCodeHelper.GetStatusResponse(500, (UserResponse?)null);
                return errorResponse;
            }
        }

        [HttpPost("Refresh")]
        public async Task<ApiResponseType<UserResponse?>> RefreshToken([FromBody] TokenRequest request)
        {
            _logger.LogInformation($"{nameof(RefreshToken)}: UserController.");

            try
            {
                var data = await _userService.RefreshTokenAsync(request.Token, request.RefreshToken);
                if (data == null)
                {
                    var errorResponse = StatusCodeHelper.GetStatusResponse(3,data);
                    return errorResponse;
                }
                _logger.LogInformation("Token refreshed successfully for user {UsernameOrEmail}", data.userName);
                var response = StatusCodeHelper.GetStatusResponse(200, data);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(RefreshToken)}: UserController.");
                var errorResponse = StatusCodeHelper.GetStatusResponse(500, (UserResponse?)null);
                return errorResponse;
            }
        }

        [HttpPost("request-otp")]
        public async Task<ApiResponseType<PasswordResetResponse?>> RequestOtp([FromBody] string userNameOrEmail)
        {
            _logger.LogInformation($"{nameof(RequestOtp)}: UserController.");

            try
            {
                var data = await _userService.RequestOtpAsync(userNameOrEmail);
                if (data == null)
                {
                    return StatusCodeHelper.GetStatusResponse(400, (PasswordResetResponse?)null);
                }
                return StatusCodeHelper.GetStatusResponse(200, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(RequestOtp)}: UserController.");
                return StatusCodeHelper.GetStatusResponse(500, (PasswordResetResponse?)null);
            }
        }

        [HttpPost("reset")]
        public async Task<ApiResponse> ResetPasswordAsync([FromBody] ResetPassRequest requestDto)
        {
            _logger.LogInformation($"{nameof(ResetPasswordAsync)}: UserController.");

            try
            {
                var isSuccess = await _userService.ResetPasswordAsync(requestDto.resetToken, requestDto.Otp, requestDto.NewPassword);
                if (!isSuccess)
                {
                    return StatusCodeHelper.GetStatusResponseWithoutType(400);
                }

                return StatusCodeHelper.GetStatusResponseWithoutType(200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ResetPasswordAsync)}: UserController.");
                return StatusCodeHelper.GetStatusResponseWithoutType(500);
            }
        }

        [HttpPost("Logout")]
        public async Task<ApiResponse> Logout([FromBody] LogoutRequest request)
        {
            _logger.LogInformation($"{nameof(Logout)}: UserController.");

            try
            {
                await _userService.LogoutUserAsync(request.token);
                var response= StatusCodeHelper.GetStatusResponseWithoutType(200);
                return response;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"{nameof(Logout)}: UserController.");
                var errorResponse = StatusCodeHelper.GetStatusResponseWithoutType(500);
                return errorResponse;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTOs.Request;
using Common.DTOs.Response;
using Common.Models;


namespace Business
{
    public interface IUserService
    {
        Task<UserResponse?> AuthenticateUserAsync(LoginReq request);
        Task LogoutUserAsync(string token);
        Task<UserResponse?> RefreshTokenAsync(string token, string refreshToken);
        Task<bool?> CheckUserExistsAsync(int userId);
        Task<PasswordResetResponse?> CreatePasswordResetTokenAsync(string userNameOrEmail);
        Task<bool> ResetPasswordAsync(string resetToken, string otp, string newPassword);
        Task<bool?> CheckUserExistsByUserNameOrEmailAsync(string userNameOrEmail);
        Task<PasswordResetResponse?> RequestOtpAsync(string userNameOrEmail);
        Task<bool> ValidateOtpAsync(Guid userId, string otp);
    }
}

using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameOrEmailAsync(string identifier);
        Task<UserToken?> GetUserByTokenAsync(string token);
        Task UpdateUserAsync(User user);
        Task UpdateUserTokenAsync(Guid userId, string token, string refreshToken);
        Task DeleteUserTokensAsync(List<UserToken> userTokens);
        Task<List<UserToken>> GetUserTokensByUserIdAsync(Guid userId);
        Task<string?> GetRoleNameByRoleIdAsync(int roleId);
        Task<bool?> CheckUserExistsAsync(int userId);
        Task<string?> CreatePasswordResetTokenAsync(string userNameOrEmail);
        Task<User?> GetUserByResetTokenAsync(string resetToken);
        Task UpdateUserTokenMethodAsync(Guid userId, UserToken? userToken);
        Task<bool?> CheckUserExistsByUserNameOrEmailAsync(string identifier);
        Task StoreOtpAsync(Guid userId, string otp, string resetToken);
    }
}

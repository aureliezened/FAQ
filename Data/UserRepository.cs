using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using Common.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace Data
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly JwtConfig _jwtConfig;
        private readonly ILogger<UserRepository> _logger;


        public UserRepository(AppDbContext context, IOptions<JwtConfig> jwtConfig, ILogger<UserRepository> logger)
        {
            _context = context;
            _jwtConfig = jwtConfig.Value;
            _logger = logger;
        }

        public async Task<User?> GetUserByUsernameOrEmailAsync(string identifier)
        {
            _logger.LogInformation($"{nameof(GetUserByUsernameOrEmailAsync)}: UserRepository.");
            try
            {
                return await _context.Users
                    .Include(u => u.userToken)  
                    .FirstOrDefaultAsync(u => u.userName == identifier || u.email == identifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetUserByUsernameOrEmailAsync)}: UserRepository.");
                throw;
            }
        }

        public async Task<string?> GetRoleNameByRoleIdAsync(int roleId)
        {
            _logger.LogInformation($"{nameof(GetRoleNameByRoleIdAsync)}: UserRepository.");
            try
            {
                var roleName = await _context.Roles
                    .FromSqlRaw("SELECT getRoleNameByRoleId({0}) AS \"roleName\"", roleId)
                    .Select(r => r.roleName)
                    .FirstOrDefaultAsync();

                return roleName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetRoleNameByRoleIdAsync)}: UserRepository.");
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            _logger.LogInformation($"{nameof(UpdateUserAsync)}: UserRepository.");
            try
            {
                user.joinedAt = user.joinedAt.ToUniversalTime();
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UpdateUserAsync)}: UserRepository.");
                throw;
            }
        }

        public async Task UpdateUserTokenAsync(Guid userId, string token, string refreshToken)
        {
            _logger.LogInformation($"{nameof(UpdateUserTokenAsync)}: UserRepository.");
            try
            {
                var userToken = await _context.UserTokens.FirstOrDefaultAsync(ut => ut.userId == userId);
                if (userToken != null)
                {
                    userToken.token = token;
                    userToken.tokenExpiryTime = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryTimeFrame.TotalMinutes);
                    userToken.refreshToken = refreshToken;
                    userToken.refreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(_jwtConfig.RefreshTokenExpiryTimeFrame.TotalMinutes);
                    _context.UserTokens.Update(userToken);
                }
                else
                {
                    userToken = new UserToken
                    {
                        userId = userId,
                        token = token,
                        tokenExpiryTime = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryTimeFrame.TotalMinutes),
                        refreshToken = refreshToken,
                        refreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(_jwtConfig.RefreshTokenExpiryTimeFrame.TotalMinutes)
                    };
                    await _context.UserTokens.AddAsync(userToken);
                }
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UpdateUserTokenAsync)}: UserRepository.");
                throw;
            }
        }

        public async Task<UserToken?> GetUserByTokenAsync(string token)
        {
            _logger.LogInformation($"{nameof(GetUserByTokenAsync)}: UserRepository.");
            try
            {
                return await _context.UserTokens
                    .Include(ut =>ut.user)
                    .FirstOrDefaultAsync(ut => ut.token == token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetUserByTokenAsync)}: UserRepository.");
                throw;
            }
        }

        public async Task<List<UserToken>> GetUserTokensByUserIdAsync(Guid userId)
        {
            _logger.LogInformation($"{nameof(GetUserTokensByUserIdAsync)}: UserRepository.");
            try
            {
                return await _context.UserTokens.Where(ut => ut.userId == userId).ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetUserTokensByUserIdAsync)}: UserRepository.");
                throw; 
            }
        }

        public async Task<User?> GetUserByResetTokenAsync(string resetToken)
        {
            _logger.LogInformation($"{nameof(GetUserByResetTokenAsync)}: UserRepository.");
            try
            {
                var userToken = await _context.UserTokens
                    .FirstOrDefaultAsync(ut => ut.resetToken == resetToken && ut.resetTokenExpiry > DateTime.UtcNow);

                if (userToken == null)
                {
                    return null; 
                }

                return await _context.Users
                    .FirstOrDefaultAsync(u => u.userId == userToken.userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetUserByResetTokenAsync)}: UserRepository.");
                throw;
            }
        }

        public async Task DeleteUserTokensAsync(List<UserToken> userTokens)
        {
            _logger.LogInformation($"{nameof(DeleteUserTokensAsync)}: UserRepository.");
            try
            {
                _context.UserTokens.RemoveRange(userTokens);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(DeleteUserTokensAsync)}: UserRepository.");
                throw;
            }
        }

        public async Task<bool?> CheckUserExistsAsync(int userId)
        {
            _logger.LogInformation($"{nameof(CheckUserExistsAsync)}: UserRepository.");

            try
            {
                await _context.Database.OpenConnectionAsync();

                var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "SELECT check_user_exists(@p_user_id)";
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.Add(new NpgsqlParameter("@p_user_id", NpgsqlTypes.NpgsqlDbType.Integer) { Value = userId });

                using (command)
                {
                    var obj = await command.ExecuteScalarAsync();
                    if (obj != null && bool.TryParse(obj.ToString(), out bool ans))
                    {
                        return ans;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CheckUserExistsAsync)}: UserRepository.");
                throw;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<bool?> CheckUserExistsByUserNameOrEmailAsync(string identifier)
        {
            _logger.LogInformation($"{nameof(CheckUserExistsByUserNameOrEmailAsync)}: UserRepository.");

            try
            {
                await _context.Database.OpenConnectionAsync();

                var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "SELECT is_valid_user(@identifier)";
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.Add(new NpgsqlParameter("@identifier", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = identifier });

                using (command)
                {
                    var obj = await command.ExecuteScalarAsync();
                    if (obj != null && bool.TryParse(obj.ToString(), out bool ans))
                    {
                        return ans;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CheckUserExistsByUserNameOrEmailAsync)}: UserRepository.");
                throw;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<string?> CreatePasswordResetTokenAsync(string userNameOrEmail)
        {
            _logger.LogInformation($"{nameof(CreatePasswordResetTokenAsync)}: UserRepository.");
            try
            {
                await _context.Database.OpenConnectionAsync();

                DbCommand cmd = _context.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = "Select Generate_Password_Reset_Token(@userNameOrEmail)";
                cmd.Parameters.Add(new NpgsqlParameter("@userNameOrEmail", NpgsqlTypes.NpgsqlDbType.Text) { Value = userNameOrEmail });

                using (cmd) 
                { 
                    var obj = await cmd.ExecuteScalarAsync();
                    if (obj != null && obj != DBNull.Value)
                    {
                        return obj.ToString();
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CreatePasswordResetTokenAsync)}: UserRepository.");
                throw;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task UpdateUserTokenMethodAsync(Guid userId, UserToken? userToken)
        {
            _logger.LogInformation($"{nameof(UpdateUserTokenMethodAsync)}: UserRepository.");
            try
            {
                var existingUserToken = await _context.UserTokens.FirstOrDefaultAsync(ut => ut.userId == userId);
                if (existingUserToken != null)
                {
                    existingUserToken.resetToken = userToken?.resetToken;
                    existingUserToken.resetTokenExpiry = userToken?.resetTokenExpiry;
                    _context.UserTokens.Update(existingUserToken);
                }
                else if (userToken != null)
                {
                    await _context.UserTokens.AddAsync(userToken);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UpdateUserTokenMethodAsync)}: UserRepository.");
                throw;
            }
        }

        public async Task StoreOtpAsync(Guid userId, string otp, string resetToken)
        {
            _logger.LogInformation($"{nameof(StoreOtpAsync)}: UserRepository.");
            try
            {
                var existingToken = await _context.UserTokens.FirstOrDefaultAsync(ut => ut.userId == userId);
                if (existingToken != null)
                {
                    existingToken.otp = otp;
                    existingToken.otpExpiry = DateTime.UtcNow.AddMinutes(5); 
                    existingToken.resetToken = resetToken;
                    existingToken.resetTokenExpiry = DateTime.UtcNow.AddHours(1); 
                    _context.UserTokens.Update(existingToken);
                }
                else
                {
                    var userToken = new UserToken
                    {
                        userId = userId,
                        otp = otp,
                        otpExpiry = DateTime.UtcNow.AddMinutes(5),
                        resetToken = resetToken,
                        resetTokenExpiry = DateTime.UtcNow.AddHours(1)
                    };
                    await _context.UserTokens.AddAsync(userToken);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(StoreOtpAsync)}: UserRepository.");
                throw;
            }
        }

        private string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Generate a 6-digit OTP
        }
    }
}

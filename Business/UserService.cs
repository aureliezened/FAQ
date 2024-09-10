using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Common.DTOs.Request;
using Common.DTOs.Response;
using Common.Models;
using Common.Configuration;
using Data;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using static System.Net.WebRequestMethods;


namespace Business
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtConfig _jwtConfig;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailService _emailService;


        public UserService(IUserRepository userRepository, IOptions<JwtConfig> jwtConfig, ILogger<UserService> logger, IEmailService emailService)
        {
            _userRepository = userRepository;
            _jwtConfig = jwtConfig.Value;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<UserResponse?> AuthenticateUserAsync(LoginReq request)
        {
            _logger.LogInformation($"{nameof(AuthenticateUserAsync)}: UserService.");
            try
            {
                var user = await _userRepository.GetUserByUsernameOrEmailAsync(request.UsernameOrEmail);
                if (user == null || !VerifyPassword(user.password, request.Password))
                {
                    return null; 
                }

                var token = await GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                await _userRepository.UpdateUserTokenAsync(user.userId, token, refreshToken);

                return new UserResponse
                {
                    userIdentifier = user.userIdentifier,
                    userName = user.userName,
                    token = token,
                    refreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AuthenticateUserAsync)}: UserService.");
                throw;
            }
        }

        private async Task <string> GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var roleName = await _userRepository.GetRoleNameByRoleIdAsync(user.roleId);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.userName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("Id",user.userId.ToString()),    
                    new Claim(ClaimTypes.Role,roleName ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryTimeFrame.TotalMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<UserResponse?> RefreshTokenAsync(string token, string refreshToken)
        {
            _logger.LogInformation($"{nameof(RefreshTokenAsync)}: UserService.");
            try
            {
                var userToken = await _userRepository.GetUserByTokenAsync(token);

                if (userToken == null || userToken.refreshToken != refreshToken || userToken.refreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return null;
                }

                var newToken = await GenerateJwtToken(userToken.user);
                var newRefreshToken = GenerateRefreshToken();

                await _userRepository.UpdateUserTokenAsync(userToken.userId, newToken, newRefreshToken);

                return new UserResponse
                {
                    userIdentifier = userToken.user.userIdentifier,
                    userName = userToken.user.userName,
                    token = newToken,
                    refreshToken = newRefreshToken
                };

            }
            catch (Exception ex) {

                _logger.LogError(ex, $"{nameof(RefreshTokenAsync)}: UserService.");
                throw;
            } 
        }

        public async Task LogoutUserAsync(string token)
        {
            _logger.LogInformation($"{nameof(LogoutUserAsync)}: UserService.");
            try
            {
                var userToken = await _userRepository.GetUserByTokenAsync(token);
                if (userToken != null)
                {
                    var userTokens = await _userRepository.GetUserTokensByUserIdAsync(userToken.userId);
                    await _userRepository.DeleteUserTokensAsync(userTokens);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(LogoutUserAsync)}: UserService.");
                throw;
            }
        }

        public async Task<bool?> CheckUserExistsAsync(int userId)
        {
            _logger.LogInformation($"{nameof(CheckUserExistsAsync)}: UserService.");
            try
            {

                return await _userRepository.CheckUserExistsAsync(userId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CheckUserExistsAsync)}: UserService.");
                throw;
            }
        }

        public async Task<bool?> CheckUserExistsByUserNameOrEmailAsync(string userNameOrEmail)
        {
            _logger.LogInformation($"{nameof(CheckUserExistsByUserNameOrEmailAsync)}: UserService.");
            try
            {
                return await _userRepository.CheckUserExistsByUserNameOrEmailAsync(userNameOrEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CheckUserExistsByUserNameOrEmailAsync)}: UserService.");
                throw;
            }
        }

        public async Task<PasswordResetResponse?> CreatePasswordResetTokenAsync(string userNameOrEmail)
        {
            _logger.LogInformation($"{nameof(CreatePasswordResetTokenAsync)}: UserService.");

            try
            {
                var resetToken = await _userRepository.CreatePasswordResetTokenAsync(userNameOrEmail);

                if (resetToken == null)
                {
                    return null; 
                }

                var user = await _userRepository.GetUserByResetTokenAsync(resetToken);

                if (user == null)
                {
                    return null;
                }

                return new PasswordResetResponse
                {
                    userIdentifier = user.userIdentifier,
                    userName = user.userName,
                    resetToken = resetToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CreatePasswordResetTokenAsync)}: UserService.");
                throw; 
            }
        }

        public async Task<bool> ResetPasswordAsync(string resetToken, string otp, string newPassword)
        {
            _logger.LogInformation($"{nameof(ResetPasswordAsync)}: UserService.");

            try
            {
                var user = await _userRepository.GetUserByResetTokenAsync(resetToken);
                if (user == null || !await ValidateOtpAsync(user.userId, otp))
                {
                    return false; 
                }

                user.password = HashPassword(newPassword);
                await _userRepository.UpdateUserAsync(user);

                // Delete the OTP after successful password reset
                var userTokens = await _userRepository.GetUserTokensByUserIdAsync(user.userId);
                var otpToken = userTokens.FirstOrDefault(ut => ut.otp == otp);
                if (otpToken != null)
                {
                    await _userRepository.DeleteUserTokensAsync(new List<UserToken> { otpToken });
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ResetPasswordAsync)}: UserService.");
                throw;
            }
        }

        private string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Generate a 6-digit OTP
        }

        public async Task<PasswordResetResponse?> RequestOtpAsync(string userNameOrEmail)
        {
            _logger.LogInformation($"{nameof(RequestOtpAsync)}: Requesting OTP for {userNameOrEmail}.");
            try
            {
                var user = await _userRepository.GetUserByUsernameOrEmailAsync(userNameOrEmail);
                if (user == null) return null;

                string otp = GenerateOtp();
                await _emailService.SendOtpToEmail(user.email, otp);

                // Generate and store the reset token
                var resetToken = await _userRepository.CreatePasswordResetTokenAsync(userNameOrEmail); 
                if (resetToken == null) return null;

                await _userRepository.StoreOtpAsync(user.userId, otp, resetToken);

                return new PasswordResetResponse
                {
                    userIdentifier = user.userIdentifier,
                    userName = user.userName,
                    otp = otp,
                    resetToken = resetToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(RequestOtpAsync)}: Error requesting OTP.");
                throw;
            }
        }

        public async Task<bool> ValidateOtpAsync(Guid userId, string otp)
        {
            var userTokens = await _userRepository.GetUserTokensByUserIdAsync(userId);
            var userToken = userTokens.FirstOrDefault(); 

            if (userToken == null || userToken.otp != otp || userToken.otpExpiry <= DateTime.UtcNow)
            {
                return false; // Invalid OTP
            }
            return true; 
        }

        private bool VerifyPassword(string hashedPassword, string password)
        {
            var hashedInputPassword = HashPassword(password);
            return hashedPassword == hashedInputPassword;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}

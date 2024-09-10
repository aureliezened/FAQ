using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class UserToken
    {
        public int tokenId { get; set; }
        public string? token { get; set; }
        public DateTime? tokenExpiryTime { get; set; }
        public string? refreshToken { get; set; }
        public DateTime? refreshTokenExpiryTime { get; set; }
        public string? resetToken { get; set; }
        public DateTime? resetTokenExpiry { get; set; }
        public string? otp { get; set; } // Add OTP property
        public DateTime? otpExpiry { get; set; } // Add OTP expiry property
        public Guid userId { get; set; }

        public User user { get; set; }
    }
}
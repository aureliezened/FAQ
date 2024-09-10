using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Response
{
    public class PasswordResetResponse
    {
        public int? userIdentifier { get; set; }
        public string userName { get; set; }
        public string otp {  get; set; }
        public string? resetToken { get; set; }
    }
}

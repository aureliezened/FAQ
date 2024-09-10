using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Request
{
    public class ResetPasswordRequest
    {
        public string UserNameOrEmail { get; set; }
        public string Otp { get; set; }
        public string NewPassword { get; set; }
    }
}

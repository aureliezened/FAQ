using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Request
{
    public class PasswordResetRequestDto
    {
        public string ResetToken { get; set; }
        public string NewPassword { get; set; }
    }
}

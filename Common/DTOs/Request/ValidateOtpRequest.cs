using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Request
{
    public class ValidateOtpRequest
    {
        public Guid UserId { get; set; }
        public string Otp { get; set; }
    }
}

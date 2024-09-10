using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Response
{
    public class UserResponse
    {
        public int? userIdentifier {  get; set; }
        public string userName { get; set; }
        public string? token { get; set; }
        public string? refreshToken { get; set; }

    }
}

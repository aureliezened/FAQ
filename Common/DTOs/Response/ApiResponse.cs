using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Response
{
    public class ApiResponse
    {
        public int statusCode { get; set; }
        public string message { get; set; }

        public ApiResponse(int statusCode, string message)
        {
            this.statusCode = statusCode;
            this.message = message;
        }
    }
}

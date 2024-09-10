using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Response
{
    public class ApiResponseType<T> // this ApiResponse can be of any type: <T>
    {
        public int statusCode {  get; set; }
        public string message { get; set; }
        public T? Data { get; set; }

        public ApiResponseType(int statusCode, string message, T? data)
        {
            this.statusCode = statusCode;
            this.message = message;
            Data = data;
        }

    }
}

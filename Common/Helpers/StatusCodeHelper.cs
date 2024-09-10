using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTOs.Response;

namespace Common.Helpers
{
    public static class StatusCodeHelper
    {
        public static readonly Dictionary<int, string> StatusCodes = new Dictionary<int, string>
        {
            { 200, "OK" },
            { 400, "Bad Request" },
            { 401, "Invalid username or password" },
            { 404, "Not Found" },
            { 500, "Internal Server Error" },
            { 1, "Something went wrong, please try again later." },
            { 2, "Question is null or empty." },
            {3, "Invalid token or refresh token." },
            {4, "Invalid token." },
            {5, "No Record Found."},
            {6, "Invalid level." },
            {7, "Invalid User" }

        };

        public static ApiResponseType<T?> GetStatusResponse<T>(int statusCode, T data)
        {
            var message = StatusCodes.ContainsKey(statusCode) ? StatusCodes[statusCode] : "Unknown Status Code";
            return new ApiResponseType<T?>(statusCode, message, data);
        }

        public static ApiResponseType<T> GetStatusResponseNotNull<T>(int statusCode, T data)
        {
            var message = StatusCodes.ContainsKey(statusCode) ? StatusCodes[statusCode] : "Unknown Status Code";
            return new ApiResponseType<T>(statusCode, message, data);
        }

        public static ApiResponse GetStatusResponseWithoutType(int statusCode)
        {
            var message = StatusCodes.ContainsKey(statusCode) ? StatusCodes[statusCode] : "Unknown Status Code";
            return new ApiResponse(statusCode, message);
        }

        public static ApiResponseType<IEnumerable<T?>?> GetStatusResponseIEnumerable<T>(int statusCode, IEnumerable<T?>? data)
        {
            var message = StatusCodes.ContainsKey(statusCode) ? StatusCodes[statusCode] : "Unknown Status Code";
            return new ApiResponseType<IEnumerable<T?>?>(statusCode, message, data);
        }

    }
}

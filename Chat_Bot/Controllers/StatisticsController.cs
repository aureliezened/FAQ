using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Business;
using Common.DTOs.Response;
using Common.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Chat_Bot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly ILogger<StatisticsController> _logger;
        private readonly IUserService _userService;


        public StatisticsController(IStatisticsService statisticsService,IUserService userService, ILogger<StatisticsController> logger)
        {
            _statisticsService = statisticsService;
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        ///  The front end should handle the case when the the user enters a level, the specific filtering should appear and the remaining should be hidden.
        /// If level 3: the admin can specify which year or the range of years or nothing (to get all years).
        /// If level 2: the admin should specify the year (required) + can specify the month or the range of months or nothing (to get all months).
        /// If level 1: the admin should specify the year (required) and the month (required) + can specify the day or the range of days or nothing (to get all days).
        /// All the options above can be executed with a specific user or all users if the input of userId is null.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="userId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <param name="startMonth"></param>
        /// <param name="endMonth"></param>
        /// <param name="startDay"></param>
        /// <param name="endDay"></param>
        /// <returns></returns>
        [HttpGet("Statistics-Sum")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Moderator")]

        public async Task<ApiResponseType<StatisticsSumResponse?>> GetSumByLevelAndUser(
            int level,
            int? userId,
            int? year = null,
            int? month = null,
            int? day = null,
            int? startYear = null,
            int? endYear = null,
            int? startMonth = null,
            int? endMonth = null,
            int? startDay = null,
            int? endDay = null)
        {
            _logger.LogInformation($"{nameof(GetSumByLevelAndUser)} : StatisticsController.");

            try
            {
                if (userId.HasValue)
                {
                    bool? userExists = await _userService.CheckUserExistsAsync(userId.Value);
                    if ((bool)!userExists)
                    {
                        var notFoundResponse = StatusCodeHelper.GetStatusResponse(7, (StatisticsSumResponse?)null);
                        return notFoundResponse;
                    }
                }

                int? levelId = await _statisticsService.IsValidLevel(level);
                if (levelId == null)
                {
                    var errorResponse = StatusCodeHelper.GetStatusResponse(6, (StatisticsSumResponse?)null);
                    return errorResponse;
                }

                var statisticsSum = await _statisticsService.GetStatisticsSumByLevelAndUserAsync(level, userId, year, month, day, startYear, endYear, startMonth, endMonth, startDay, endDay);

                var response = StatusCodeHelper.GetStatusResponse(200, statisticsSum);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetSumByLevelAndUser)}: StatisticsController.");
                var errorResponse = StatusCodeHelper.GetStatusResponseNotNull(1, (StatisticsSumResponse?)null);
                return errorResponse;
            }
        }
    }
}

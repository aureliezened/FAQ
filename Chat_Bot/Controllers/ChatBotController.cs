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


namespace Chat_Bot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    { 
        private readonly ILogger<ChatBotController> _logger;
        private readonly IChatService _chatService;
        private readonly IStatisticsService _statisticsService;
        private readonly IUserService _userService; 

        //constructor 
        public ChatBotController(IChatService chatService, ILogger<ChatBotController> logger, IStatisticsService statisticsService, IUserService userService)
        {
            _chatService = chatService;
            _logger = logger;
            _statisticsService = statisticsService;
            _userService = userService;
        }


        [HttpPost("Ask-Question")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ApiResponseType<AnswerResponseDto>> AskQuestion([FromBody] string question) //add userIdentifier
        {
            var errorStaticResponseDto = new AnswerResponseDto();
            errorStaticResponseDto.answer = "Something went wrong, please try again later...";

            _logger.LogInformation($"{nameof(AskQuestion)}: ChatBotController.");

            if (string.IsNullOrWhiteSpace(question))
            {
                var errorResponse = StatusCodeHelper.GetStatusResponseNotNull(2, errorStaticResponseDto);
                return errorResponse;
            }

            try
            {
                var userId = User.FindFirstValue("Id");
                var answer = await _chatService.ProcessQuestionAsync(Guid.Parse(userId), question);

                var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
                await _statisticsService.UpdateStatisticsAsync(Guid.Parse(userId), currentDate);

                var response = StatusCodeHelper.GetStatusResponseNotNull(200, answer);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AskQuestion)}: ChatBotController.");
                var errorResponse = StatusCodeHelper.GetStatusResponseNotNull(1, errorStaticResponseDto);
                return errorResponse;
            }
        }

        

        [HttpGet("All-Chat-History")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Moderator")]
        public async Task<ApiResponseType<IEnumerable<ChatHistoryResponseDto?>?>> GetChatHistory()
        {
            _logger.LogInformation($"{nameof(GetChatHistory)}: ChatBotController.");

            try
            {
                var chatHistory = await _chatService.GetAllChatHistoryAsync();
                var response = StatusCodeHelper.GetStatusResponseIEnumerable(200,chatHistory);
                return response;

            }
            catch (Exception ex) {
                _logger.LogError(ex, $"{nameof(GetChatHistory)}: ChatBotController.");
                var errorResponse= StatusCodeHelper.GetStatusResponseIEnumerable(1, (IEnumerable<ChatHistoryResponseDto?>?)null);
                return errorResponse;
            }
            
        }

        [HttpGet("All-Users")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Moderator")]
        public async Task<ApiResponseType<IEnumerable<UsersResponseDto?>?>> GetUsers()
        {
            _logger.LogInformation($"{nameof(GetUsers)}: ChatBotController.");
            try
            {
                var users = await _chatService.GetAllUsersAsync();
                var response = StatusCodeHelper.GetStatusResponseIEnumerable(200, users);
                return response;

            }catch(Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetUsers)}: ChatBotController.");
                var errorResponse = StatusCodeHelper.GetStatusResponseIEnumerable(1, (IEnumerable<UsersResponseDto?>?)null);
                return errorResponse;
            }
            
        }

        [HttpGet("User-Chat-History")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Moderator")]
        public async Task<ApiResponseType<IEnumerable<UserHistoryResponseDto?>?>> GetUserHistory(int userId)
        {
            _logger.LogInformation($"{nameof(GetUserHistory)}: ChatBotController.");

            try
            {
               bool? userExists = await _userService.CheckUserExistsAsync(userId);
               if ((bool)!userExists)
               {
                    var notFoundResponse = StatusCodeHelper.GetStatusResponse(7, (IEnumerable<UserHistoryResponseDto?>?)null);
                    return notFoundResponse;
               }
                
                var userHistory = await _chatService.GetUserHistoryAsync(userId);
                var response = StatusCodeHelper.GetStatusResponseIEnumerable(200, userHistory);
                return response;
               
            }catch(Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetUserHistory)}: ChatBotController.");
                var errorResponse = StatusCodeHelper.GetStatusResponseIEnumerable(1, (IEnumerable<UserHistoryResponseDto?>?)null);
                return errorResponse;
            }
        }

    }
}

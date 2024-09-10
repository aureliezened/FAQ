
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Data;
using Common.Models;
using Common.DTOs.Response;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace Business
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly HttpClient _httpClient; 
        private readonly ILogger<ChatService> _logger;
        private readonly IUserRepository _userRepository;

        public ChatService(IChatRepository chatRepository, HttpClient httpClient, ILogger<ChatService> logger, IUserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _httpClient = httpClient;
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task<AnswerResponseDto> ProcessQuestionAsync(Guid userId, string question)
        {
            _logger.LogInformation($"{nameof(ProcessQuestionAsync)}: ChatService.");
            try
            {
                var answer = await GetAnswerFromAI(userId,question);
                if (answer == null || answer is null ) {
                  var errorAnswerResponseDto = new AnswerResponseDto();
                    errorAnswerResponseDto.answer = "Something went wrong...";
                    return errorAnswerResponseDto;
                }

                var chatHistory = new ChatHistory
                {
                    userId = userId,
                    question = question,
                    answer = answer.answer,
                    time = DateTime.UtcNow
                };

                await _chatRepository.AddChatHistoryAsync(chatHistory);
                return answer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ProcessQuestionAsync)}: ChatService.");
                throw;
            }
        }
        private async Task<AnswerResponseDto> GetAnswerFromAI(Guid userId,string question)
        {
            _logger.LogInformation($"{nameof(GetAnswerFromAI)}: ChatService.");
            try
            {
                //API endpoint in AI: Ask-Question. sending a JSON object with userId and question.
                var response = await _httpClient.PostAsJsonAsync("Ask-Question", new { userId, question });

                _logger.LogInformation($"{nameof(GetAnswerFromAI)}: Response Status Code: {response.StatusCode}");

                using var responseStream = await response.Content.ReadAsStreamAsync();

                _logger.LogInformation($"{nameof(GetAnswerFromAI)}: Raw Response Content: {responseStream}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"{nameof(GetAnswerFromAI)}: API call failed with status code {response.StatusCode} and content: {responseStream}");
                    throw new HttpRequestException($"API call failed with status code {response.StatusCode}");
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Ensure case insensitivity
                };

                var rawResponseContent = await new StreamReader(responseStream).ReadToEndAsync();
                _logger.LogInformation($"{nameof(GetAnswerFromAI)}: Raw Response Content: {rawResponseContent}");

                // Reset the stream position to 0 for deserialization
                responseStream.Position = 0;

                var answerDto = await JsonSerializer.DeserializeAsync<AnswerResponseDto>(responseStream, options);

                return answerDto;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"{nameof(GetAnswerFromAI)}: HTTP request error.");
                throw; 
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"{nameof(GetAnswerFromAI)}: Error deserializing JSON.");
                throw; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetAnswerFromAI)}: Unknown error.");
                throw; 
            }
        }

        public async Task<IEnumerable<ChatHistoryResponseDto>> GetAllChatHistoryAsync()
        {
            _logger.LogInformation($"{nameof(GetAllChatHistoryAsync)}: ChatService.");
            try
            {
                return await _chatRepository.GetAllChatHistoryAsync();

            }catch(Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetAllChatHistoryAsync)}: ChatService.");
                throw;
            }
        }

        public async Task <IEnumerable<UsersResponseDto>> GetAllUsersAsync()
        {
            _logger.LogInformation($"{nameof(GetAllUsersAsync)}: ChatService.");
            try
            {
                return await _chatRepository.GetAllUsersAsync();

            }catch(Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetAllUsersAsync)}: ChatService.");
                throw;

            }
        }

        public async Task<IEnumerable<UserHistoryResponseDto>> GetUserHistoryAsync(int userId)
        {
            _logger.LogInformation($"{nameof(GetUserHistoryAsync)}: ChatService.");
            try { 
                return await _chatRepository.GetUserHistoryAsync(userId);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetUserHistoryAsync)}: ChatService.");
                throw;
            }
        }
    }
}

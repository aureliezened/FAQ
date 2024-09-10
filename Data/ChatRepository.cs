using Common.DTOs.Response;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ChatRepository> _logger;

        public ChatRepository(AppDbContext dbContext, ILogger<ChatRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
     
        public async Task AddChatHistoryAsync(ChatHistory chatHistory)
        {
            _logger.LogInformation($"{nameof(AddChatHistoryAsync)}: ChatRepository.");
            try
            {
                _dbContext.ChatHistories.Add(chatHistory);
                await _dbContext.SaveChangesAsync();
            }catch(Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AddChatHistoryAsync)}: ChatRepository.");
                throw;
            }
        }
        public async Task<IEnumerable<ChatHistoryResponseDto>> GetAllChatHistoryAsync()
        {
            _logger.LogInformation($"{nameof(GetAllChatHistoryAsync)}: ChatRepository.");
            try
            {
                  var chatHistory = await _dbContext.ChatHistories
                    .FromSqlRaw("SELECT * FROM get_chat_histories()")
                    .Select(ch => new ChatHistoryResponseDto
                    {
                        historyId = ch.historyId,
                        question = ch.question,
                        answer = ch.answer,
                        userId = ch.userId,
                        time = ch.time
                    })
                    .ToListAsync();

                return chatHistory;

            }catch(Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetAllChatHistoryAsync)}: ChatRepository.");
                throw;
            }
            
        }
        public async Task<IEnumerable<UsersResponseDto>> GetAllUsersAsync()
        {
            _logger.LogInformation($"{nameof(GetAllUsersAsync)}: ChatRepository.");

            try
                {
                var users = await _dbContext.Users
                    .FromSqlRaw("SELECT * FROM get_all_users()")
                    .Select(u => new UsersResponseDto
                    {
                        userIdentifier= u.userIdentifier,
                        userName= u.userName
                    })
                    .ToListAsync();
                return users;

            }catch(Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetAllUsersAsync)}: ChatRepository.");
                throw;
            }
        }
        public async Task<IEnumerable<UserHistoryResponseDto>> GetUserHistoryAsync(int userId)
        {
            _logger.LogInformation($"{nameof(GetUserHistoryAsync)}: ChatRepository.");
            try
            {
                var userHistory = await _dbContext.ChatHistories
                                .FromSqlRaw("SELECT * FROM get_user_history({0})", userId)
                                .Select(u => new UserHistoryResponseDto
                                {
                                    historyId= u.historyId,
                                    question = u.question,
                                    answer = u.answer,
                                    time = u.time
                                })
                                .ToListAsync();
                return userHistory;

            } catch(Exception ex) 
            { 
                _logger.LogError(ex, $"{nameof(GetUserHistoryAsync)}: ChatRepository.");
                throw;
            }   
        }
    }
}

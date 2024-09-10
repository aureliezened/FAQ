using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTOs.Response;

namespace Data
{
    public interface IChatRepository
    {
        Task AddChatHistoryAsync(ChatHistory chatHistory);
        Task<IEnumerable<ChatHistoryResponseDto>> GetAllChatHistoryAsync();
        Task<IEnumerable<UsersResponseDto>> GetAllUsersAsync();
        Task<IEnumerable<UserHistoryResponseDto>> GetUserHistoryAsync(int userId);
    }
}

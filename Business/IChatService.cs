using Common.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public interface IChatService
    {
        Task<AnswerResponseDto> ProcessQuestionAsync(Guid userId, string question);
        Task<IEnumerable<ChatHistoryResponseDto>> GetAllChatHistoryAsync();
        Task<IEnumerable<UsersResponseDto>> GetAllUsersAsync();
        Task<IEnumerable<UserHistoryResponseDto>> GetUserHistoryAsync(int userId);
        
    }
}

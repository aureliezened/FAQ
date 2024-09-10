using System;

namespace Common.DTOs.Response
{
    public class ChatHistoryResponseDto
    {
        public int historyId { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
        public Guid userId { get; set; }
        public DateTime time { get; set; }
    }
}

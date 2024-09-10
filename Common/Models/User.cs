using System;

namespace Common.Models
{
    public class User
    {
        public Guid userId { get; set; }
        public int userIdentifier { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string? phoneNumber { get; set; }
        public string userName { get; set; }
        public int roleId { get; set; }

        private DateTime _joinedAt;
        public DateTime joinedAt
        {
            get => _joinedAt;
            set => _joinedAt = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public ICollection<ChatHistory> chatHistory { get; set; }
        public ICollection<Statistics> statistics { get; set; }

        public Role role { get; set; }
        public UserToken? userToken { get; set; }

      

    }
}

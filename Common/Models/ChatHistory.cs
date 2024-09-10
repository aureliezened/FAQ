using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class ChatHistory
    {
        public int historyId {  get; set; }
        public string question { get; set; }
        public string answer { get; set; }
        public Guid userId { get; set; }

        private DateTime _Time;
        public DateTime time
        {
            get => _Time;
            set => _Time = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        public User user { get; set; }
    }
}

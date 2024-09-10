using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Response
{
    public class UserHistoryResponseDto
    {
        public int historyId {  get; set; }
        public string question { get; set; }
        public string answer {get; set; }
        public DateTime time { get; set; }
    }
}

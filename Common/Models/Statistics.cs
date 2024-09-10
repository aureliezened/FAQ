using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Statistics
    {
        public int statisticId {  get; set; }
        public DateOnly date {  get; set; }
        public int? day {  get; set; }
        public string? month { get; set; }
        public int year { get; set; }
        public int level { get; set; }
        public int count { get; set; }
        public Guid userId { get; set; }

        public User user { get; set; }
    }
}

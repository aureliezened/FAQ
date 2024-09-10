using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs.Response
{
    public class StatisticsSumResponse
    {
      //  level int, userId uuid, total_count int

        public int level { get; set; }
        public int? userIdentifier { get; set; }
        public int sum { get; set; }    
    }
}
    
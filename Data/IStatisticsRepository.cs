using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTOs.Response;

namespace Data
{
    public interface IStatisticsRepository
    {
        Task AddStatisticsAsync(Statistics statistics);
        Task UpdateStatisticsAsync(Statistics statistics);
        Task<Statistics?> GetStatisticsByLevelAndDateAsync(int level, DateOnly date, Guid userId);
        Task<StatisticsSumResponse?> GetStatisticsSumByLevelAndUserAsync(int level, int? userId, int? year, int? month, 
            int? day, int? startYear, int? endYear, int? startMonth, int? endMonth, int? startDay, int? endDay);
        Task<int?> IsValidLevel(int level);
    }
}

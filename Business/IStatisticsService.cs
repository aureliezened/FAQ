using Common.DTOs.Response;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace Business
{
    public interface IStatisticsService
    {
        Task UpdateStatisticsAsync(Guid userId, DateOnly date);
        Task UpdateLevel1StatisticsAsync(DateOnly date, Guid userId);
        Task UpdateLevel2StatisticsAsync(DateOnly date, Guid userId);
        Task UpdateLevel3StatisticsAsync(DateOnly date, Guid userId);
        Task<StatisticsSumResponse?> GetStatisticsSumByLevelAndUserAsync(int level,int? userId, int? year, int? month,
            int? day, int? startYear, int? endYear, int? startMonth, int? endMonth, int? startDay, int? endDay);
        Task<int?> IsValidLevel(int level);
    }
}

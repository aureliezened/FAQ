using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Common.DTOs.Request;
using Common.DTOs.Response;
using Common.Models;
using Common.Configuration;
using Data;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace Business
{
    public class StatisticsService :IStatisticsService
    {
        private readonly IStatisticsRepository _statisticsRepository;
        private readonly ILogger<StatisticsService> _logger;
        private readonly IUserRepository _userRepository;

        public StatisticsService(IStatisticsRepository statisticsRepository, ILogger<StatisticsService> logger, IUserRepository userRepository)
        {
            _statisticsRepository = statisticsRepository;
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task UpdateStatisticsAsync(Guid userId, DateOnly date)
        {
            await UpdateLevel1StatisticsAsync(date, userId);
            await UpdateLevel2StatisticsAsync(date, userId);
            await UpdateLevel3StatisticsAsync(date, userId);
        }

        public async Task UpdateLevel1StatisticsAsync(DateOnly date, Guid userId)
        {
            _logger.LogInformation($"{nameof(UpdateLevel1StatisticsAsync)}: StatisticsService.");
            try
            {
                var statistics = await _statisticsRepository.GetStatisticsByLevelAndDateAsync(1, date, userId);

                if (statistics == null)
                {
                    statistics = new Statistics
                    {
                        date = date,
                        day = date.Day,
                        month = date.ToString("MMMM"),
                        year = date.Year,
                        level = 1,
                        count = 1,
                        userId = userId
                    };
                    await _statisticsRepository.AddStatisticsAsync(statistics);
                }
                else
                {
                    // Update existing statistics entry
                    statistics.count++;
                    await _statisticsRepository.UpdateStatisticsAsync(statistics);
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex,$"{nameof(UpdateLevel1StatisticsAsync)}: StatisticsService.");
                throw;
            }
        }

        public async Task UpdateLevel2StatisticsAsync(DateOnly date, Guid userId)
        {
            _logger.LogInformation($"{nameof(UpdateLevel2StatisticsAsync)}: StatitisticsService.");
            try
            {
                var statistics = await _statisticsRepository.GetStatisticsByLevelAndDateAsync(2, date, userId);

                if (statistics == null)
                {
                    statistics = new Statistics
                    {
                        date = new DateOnly(date.Year, date.Month, 1),
                        month = date.ToString("MMMM"),
                        year = date.Year,
                        level = 2,
                        count = 1,
                        userId = userId
                    };
                    await _statisticsRepository.AddStatisticsAsync(statistics);
                }
                else
                {
                    statistics.count++;
                    await _statisticsRepository.UpdateStatisticsAsync(statistics);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"{nameof(UpdateLevel2StatisticsAsync)}: StatitisticsService.");
                throw;
            }
        }

        public async Task UpdateLevel3StatisticsAsync(DateOnly date, Guid userId)
        {
            _logger.LogInformation($"{nameof(UpdateLevel3StatisticsAsync)}: StatitisticsService.");
            try
            {
                var statistics = await _statisticsRepository.GetStatisticsByLevelAndDateAsync(3, date, userId);

                if (statistics == null)
                {
                    statistics = new Statistics
                    {
                        date = new DateOnly(date.Year, 1, 1),
                        year = date.Year,
                        level = 3,
                        count = 1,
                        userId = userId
                    };
                    await _statisticsRepository.AddStatisticsAsync(statistics);
                }
                else
                {
                    statistics.count++;
                    await _statisticsRepository.UpdateStatisticsAsync(statistics);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"{nameof(UpdateLevel3StatisticsAsync)}: StatitisticsService.");
                throw;
            }
        }

        public async Task<StatisticsSumResponse?> GetStatisticsSumByLevelAndUserAsync(
            int level, int? userId, int? year, int? month,int? day, int? startYear, int? endYear,
            int? startMonth, int? endMonth, int? startDay, int? endDay
            )
        {
            _logger.LogInformation($"{nameof(GetStatisticsSumByLevelAndUserAsync)}: StatitisticsService.");
            try
            {
                return await _statisticsRepository.GetStatisticsSumByLevelAndUserAsync(
                    level, userId ,year, month,day, startYear, endYear,startMonth, endMonth,startDay,endDay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"{nameof(GetStatisticsSumByLevelAndUserAsync)}: StatitisticsService.");
                throw;
            }
        }
        public async Task<int?> IsValidLevel(int level)
        {
            _logger.LogInformation($"{nameof(IsValidLevel)}: StatisticsService.");
            try
            {
                var levelId = await _statisticsRepository.IsValidLevel(level);
                return levelId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IsValidLevel)}: StatisticsService.");
                throw;
            }
        }
    }
}

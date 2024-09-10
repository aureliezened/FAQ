using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using Common.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Npgsql;
using Common.DTOs.Response;
using System.Data.Common;


namespace Data
{

    public class StatisticsRepository : IStatisticsRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StatisticsRepository> _logger;

        public StatisticsRepository(AppDbContext context, ILogger<StatisticsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddStatisticsAsync(Statistics statistics)
        {
            _logger.LogInformation($"{nameof(AddStatisticsAsync)}: StatisticsRepository.");
            try
            {
                await _context.Statistics.AddAsync(statistics);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Statistics saved to database successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AddStatisticsAsync)}: StatisticsRepository.");
                throw;
            }
        }

        public async Task UpdateStatisticsAsync(Statistics statistics)
        {
            _logger.LogInformation($"{nameof(UpdateStatisticsAsync)}: StatisticsRepository.");
            try
            {
                _context.Statistics.Update(statistics);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Statistics updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UpdateStatisticsAsync)}: StatisticsRepository.");
                throw;
            }
        }

        public async Task<Statistics?> GetStatisticsByLevelAndDateAsync(int level, DateOnly date, Guid userId)
        {
            _logger.LogInformation($"{nameof(GetStatisticsByLevelAndDateAsync)}: StatisticsRepository.");

            try
            {

                var dateParam = new NpgsqlParameter("Date", date.ToDateTime(TimeOnly.MinValue));
                var levelParam = new NpgsqlParameter("Level", level);
                var userIdParam = new NpgsqlParameter("UserId", userId);

                var statistics = await _context.Statistics
                            .FromSqlRaw("SELECT * FROM get_statistics_by_level_and_date(@Level, @Date, @UserId)", levelParam, dateParam, userIdParam)
                            .FirstOrDefaultAsync();

                return statistics;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetStatisticsByLevelAndDateAsync)}: StatisticsRepository.");
                throw;
            }
        }

        public async Task<StatisticsSumResponse?> GetStatisticsSumByLevelAndUserAsync(
            int level, int? userId, int? year, int? month, int? day, int? startYear,
            int? endYear, int? startMonth, int? endMonth, int? startDay, int? endDay)
        {
            _logger.LogInformation($"{nameof(GetStatisticsSumByLevelAndUserAsync)}: StatisticsRepository.");

            try
            {
                await _context.Database.OpenConnectionAsync();

                var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "SELECT * FROM get_statistics_sum_by_level_and_user12(@p_level, @p_user_id, @p_year, @p_startYear, @p_endYear, @p_month, @p_startMonth, @p_endMonth, @p_day,@p_startDay, @p_endDay)";
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.Add(new NpgsqlParameter("@p_level", NpgsqlTypes.NpgsqlDbType.Integer) { Value = level });
                command.Parameters.Add(new NpgsqlParameter("@p_user_id", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (object?)userId ?? DBNull.Value });
                command.Parameters.Add(new NpgsqlParameter("@p_year", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (object?)year ?? DBNull.Value });
                command.Parameters.Add(new NpgsqlParameter("@p_month", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (object?)month ?? DBNull.Value });
                command.Parameters.Add(new NpgsqlParameter("@p_day", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (object?)day ?? DBNull.Value });
                command.Parameters.Add(new NpgsqlParameter("@p_startYear", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (object?)startYear ?? DBNull.Value });
                command.Parameters.Add(new NpgsqlParameter("@p_endYear", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (object?)endYear ?? DBNull.Value });
                command.Parameters.Add(new NpgsqlParameter("@p_startMonth", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (object?)startMonth ?? DBNull.Value });
                command.Parameters.Add(new NpgsqlParameter("@p_endMonth", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (object?)endMonth ?? DBNull.Value });
                command.Parameters.Add(new NpgsqlParameter("@p_startDay", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (object?)startDay ?? DBNull.Value });
                command.Parameters.Add(new NpgsqlParameter("@p_endDay", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (object?)endDay ?? DBNull.Value });

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var responsedto = new StatisticsSumResponse
                        {
                            level = reader.GetInt32(reader.GetOrdinal("level")),
                            userIdentifier = reader.IsDBNull(reader.GetOrdinal("userIdentifier")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("userIdentifier")),
                            sum = reader.GetInt32(reader.GetOrdinal("sum"))
                        };

                        return responsedto;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetStatisticsSumByLevelAndUserAsync)}: StatisticsRepository.");
                throw;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
        public async Task<int?> IsValidLevel(int level)
        {
            _logger.LogInformation($"{nameof(IsValidLevel)}: StatisticsRepository.");

            try
            {
                await _context.Database.OpenConnectionAsync();
                DbCommand cmd = _context.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = $"SELECT * from get_level(@level)";
                cmd.Parameters.Add(new NpgsqlParameter("@level", NpgsqlTypes.NpgsqlDbType.Integer) { Value = level });
                using (cmd)
                {
                    var obj = await cmd.ExecuteScalarAsync();
                    if (obj != null && int.TryParse(obj.ToString(), out int levelId))
                    {
                        return levelId;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IsValidLevel)}: StatisticsRepository.");
                throw;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }      
}

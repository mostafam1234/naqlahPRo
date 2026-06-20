using Domain.InterFaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services
{
    public sealed class DatabaseBackupJobRunner
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDatabaseBackupJobStore _jobStore;

        public DatabaseBackupJobRunner(IServiceScopeFactory scopeFactory, IDatabaseBackupJobStore jobStore)
        {
            _scopeFactory = scopeFactory;
            _jobStore = jobStore;
        }

        public string StartBackup()
        {
            var jobId = _jobStore.CreateJob("Backup", 0);

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var tableCount = await GetTableCountAsync(scope.ServiceProvider);
                    _jobStore.UpdateProgress(jobId, 0, tableCount, "Starting...");

                    var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();
                    var progress = new Progress<(int completed, int total, string currentItem)>(update =>
                    {
                        _jobStore.UpdateProgress(jobId, update.completed, update.total, update.currentItem);
                    });

                    var result = await backupService.CreateFullBackupSqlAsync(progress);
                    if (result.IsFailure)
                    {
                        _jobStore.FailJob(jobId, result.Error);
                        return;
                    }

                    await using var stream = result.Value;
                    var bytes = stream.ToArray();
                    var fileName = $"Naqlah_FullBackup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";
                    _jobStore.CompleteBackup(jobId, bytes, fileName);
                }
                catch (Exception ex)
                {
                    _jobStore.FailJob(jobId, ex.Message);
                }
            });

            return jobId;
        }

        public string StartRestore(Stream sqlStream)
        {
            var sqlCopy = new MemoryStream();
            sqlStream.CopyTo(sqlCopy);
            sqlCopy.Position = 0;

            var tableCount = CountTableSections(sqlCopy);
            sqlCopy.Position = 0;

            var jobId = _jobStore.CreateJob("Restore", Math.Max(tableCount, 1));

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();

                    _jobStore.UpdateProgress(jobId, 0, Math.Max(tableCount, 1), "Starting...");

                    var progress = new Progress<(int completed, int total, string currentItem)>(update =>
                    {
                        _jobStore.UpdateProgress(jobId, update.completed, update.total, update.currentItem);
                    });

                    var result = await backupService.RestoreMergeFromSqlAsync(sqlCopy, progress);
                    if (result.IsFailure)
                    {
                        _jobStore.FailJob(jobId, result.Error);
                        return;
                    }

                    _jobStore.CompleteRestore(jobId, result.Value);
                }
                catch (Exception ex)
                {
                    _jobStore.FailJob(jobId, ex.Message);
                }
            });

            return jobId;
        }

        private static async Task<int> GetTableCountAsync(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is not configured.");

            await using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync();

            const string sql = """
                SELECT COUNT(*)
                FROM sys.tables t
                INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
                WHERE t.is_ms_shipped = 0
                  AND t.temporal_type = 0
                  AND s.name NOT IN ('HangFire', 'hangfire')
                """;

            await using var command = new Microsoft.Data.SqlClient.SqlCommand(sql, connection);
            var count = (int)(await command.ExecuteScalarAsync() ?? 0);
            return count;
        }

        private static int CountTableSections(Stream sqlStream)
        {
            using var reader = new StreamReader(sqlStream, leaveOpen: true);
            var sql = reader.ReadToEnd();
            return sql.Split("-- BEGIN TABLE:", StringSplitOptions.None).Length - 1;
        }
    }
}

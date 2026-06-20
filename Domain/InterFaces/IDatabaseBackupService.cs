using CSharpFunctionalExtensions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.InterFaces
{
    public sealed class DatabaseRestoreSummary
    {
        public int TotalTables { get; init; }
        public int TablesProcessed { get; init; }
        public int TablesChanged { get; init; }
        public int RowsInserted { get; init; }
        public int RowsSkipped { get; init; }
        public int BatchesExecuted { get; init; }
    }

    public sealed class DatabaseOperationStatus
    {
        public string JobId { get; init; } = string.Empty;
        public string Operation { get; init; } = string.Empty;
        public string Phase { get; set; } = "running";
        public int ProgressPercent { get; set; }
        public string CurrentItem { get; set; } = string.Empty;
        public int CompletedItems { get; set; }
        public int TotalItems { get; set; }
        public DatabaseRestoreSummary? Summary { get; set; }
        public string? ErrorMessage { get; set; }
        public string? DownloadFileName { get; set; }
    }

    public interface IDatabaseBackupJobStore
    {
        string CreateJob(string operation, int totalItems);

        DatabaseOperationStatus? GetJob(string jobId);

        void UpdateProgress(string jobId, int completedItems, int totalItems, string currentItem);

        void CompleteBackup(string jobId, byte[] fileBytes, string fileName);

        void CompleteRestore(string jobId, DatabaseRestoreSummary summary);

        void FailJob(string jobId, string errorMessage);

        byte[]? GetBackupFile(string jobId);
    }

    public interface IDatabaseBackupService
    {
        Task<Result<MemoryStream>> CreateFullBackupSqlAsync(
            IProgress<(int completed, int total, string currentItem)>? progress = null,
            CancellationToken cancellationToken = default);

        Task<Result<DatabaseRestoreSummary>> RestoreMergeFromSqlAsync(
            Stream sqlStream,
            IProgress<(int completed, int total, string currentItem)>? progress = null,
            CancellationToken cancellationToken = default);
    }
}

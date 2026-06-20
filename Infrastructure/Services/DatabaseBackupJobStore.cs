using Domain.InterFaces;
using System.Collections.Concurrent;

namespace Infrastructure.Services
{
    public sealed class DatabaseBackupJobStore : IDatabaseBackupJobStore
    {
        private sealed class JobEntry
        {
            public DatabaseOperationStatus Status { get; init; } = new();
            public byte[]? FileBytes { get; set; }
        }

        private readonly ConcurrentDictionary<string, JobEntry> _jobs = new();

        public string CreateJob(string operation, int totalItems)
        {
            var jobId = Guid.NewGuid().ToString("N");
            _jobs[jobId] = new JobEntry
            {
                Status = new DatabaseOperationStatus
                {
                    JobId = jobId,
                    Operation = operation,
                    Phase = "running",
                    TotalItems = totalItems,
                    ProgressPercent = 0
                }
            };
            return jobId;
        }

        public DatabaseOperationStatus? GetJob(string jobId)
        {
            return _jobs.TryGetValue(jobId, out var entry) ? entry.Status : null;
        }

        public void UpdateProgress(string jobId, int completedItems, int totalItems, string currentItem)
        {
            if (!_jobs.TryGetValue(jobId, out var entry))
                return;

            entry.Status.CompletedItems = completedItems;
            entry.Status.TotalItems = totalItems;
            entry.Status.CurrentItem = currentItem;
            entry.Status.ProgressPercent = totalItems <= 0
                ? 0
                : Math.Min(100, (int)Math.Round(completedItems * 100.0 / totalItems));
        }

        public void CompleteBackup(string jobId, byte[] fileBytes, string fileName)
        {
            if (!_jobs.TryGetValue(jobId, out var entry))
                return;

            entry.FileBytes = fileBytes;
            entry.Status.Phase = "completed";
            entry.Status.ProgressPercent = 100;
            entry.Status.DownloadFileName = fileName;
            entry.Status.CurrentItem = fileName;
        }

        public void CompleteRestore(string jobId, DatabaseRestoreSummary summary)
        {
            if (!_jobs.TryGetValue(jobId, out var entry))
                return;

            entry.Status.Phase = "completed";
            entry.Status.ProgressPercent = 100;
            entry.Status.Summary = summary;
            entry.Status.CurrentItem = string.Empty;
        }

        public void FailJob(string jobId, string errorMessage)
        {
            if (!_jobs.TryGetValue(jobId, out var entry))
                return;

            entry.Status.Phase = "failed";
            entry.Status.ErrorMessage = errorMessage;
        }

        public byte[]? GetBackupFile(string jobId)
        {
            return _jobs.TryGetValue(jobId, out var entry) ? entry.FileBytes : null;
        }
    }
}

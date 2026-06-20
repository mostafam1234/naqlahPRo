using Domain.InterFaces;

namespace Infrastructure.Services
{
    public sealed class BackupOperationService : IBackupOperationService
    {
        private readonly DatabaseBackupJobRunner _jobRunner;
        private readonly IDatabaseBackupJobStore _jobStore;

        public BackupOperationService(DatabaseBackupJobRunner jobRunner, IDatabaseBackupJobStore jobStore)
        {
            _jobRunner = jobRunner;
            _jobStore = jobStore;
        }

        public string StartBackup() => _jobRunner.StartBackup();

        public string StartRestore(Stream sqlStream) => _jobRunner.StartRestore(sqlStream);

        public DatabaseOperationStatus? GetStatus(string jobId) => _jobStore.GetJob(jobId);

        public byte[]? GetBackupFile(string jobId) => _jobStore.GetBackupFile(jobId);
    }
}

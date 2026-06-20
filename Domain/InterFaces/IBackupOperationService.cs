namespace Domain.InterFaces
{
    public interface IBackupOperationService
    {
        string StartBackup();

        string StartRestore(Stream sqlStream);

        DatabaseOperationStatus? GetStatus(string jobId);

        byte[]? GetBackupFile(string jobId);
    }
}

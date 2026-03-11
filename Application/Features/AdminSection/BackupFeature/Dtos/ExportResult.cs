namespace Application.Features.AdminSection.BackupFeature.Dtos
{
    public sealed class ExportResult : IDisposable
    {
        public Stream Stream { get; }
        public string FileName { get; }
        public string ContentType { get; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ExportResult(Stream stream, string fileName)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        public void Dispose() => Stream?.Dispose();
    }
}

namespace Application.Features.AdminSection.BackupFeature.Dtos
{
    public sealed class ExportResult : IDisposable
    {
        public Stream Stream { get; }
        public string FileName { get; }
        public string ContentType { get; }

        public ExportResult(Stream stream, string fileName, string? contentType = null)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            ContentType = contentType ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }

        public void Dispose() => Stream?.Dispose();
    }
}

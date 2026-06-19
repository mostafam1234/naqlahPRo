using Application.Features.AdminSection.BackupFeature.Abstractions;
using Application.Features.AdminSection.BackupFeature.Constants;
using Application.Features.AdminSection.BackupFeature.Dtos;
using CSharpFunctionalExtensions;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.BackupFeature.Queries
{
    public sealed class ExportModuleToExcelQueryHandler : IRequestHandler<ExportModuleToExcelQuery, Result<ExportResult>>
    {
        private readonly IModuleExporter[] _exporters;

        public ExportModuleToExcelQueryHandler(IEnumerable<IModuleExporter> exporters)
        {
            _exporters = exporters?.ToArray() ?? Array.Empty<IModuleExporter>();
        }

        public async Task<Result<ExportResult>> Handle(ExportModuleToExcelQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ModuleKey))
                return Result.Failure<ExportResult>("ModuleKeyRequired");

            if (!BackupModuleKeys.All.Contains(request.ModuleKey))
                return Result.Failure<ExportResult>("UnknownModule");

            var exporter = _exporters.FirstOrDefault(e =>
                string.Equals(e.ModuleKey, request.ModuleKey, StringComparison.OrdinalIgnoreCase));

            if (exporter == null)
                return Result.Failure<ExportResult>("NoExporterForModule");

            return await exporter.ExportAsync(request, cancellationToken);
        }
    }
}

using Application.Features.AdminSection.BackupFeature.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.BackupFeature.Queries
{
    public sealed class ExportFullDatabaseBackupQuery : IRequest<Result<ExportResult>>
    {
    }

    public sealed class ExportFullDatabaseBackupQueryHandler : IRequestHandler<ExportFullDatabaseBackupQuery, Result<ExportResult>>
    {
        private readonly IDatabaseBackupService _databaseBackupService;

        public ExportFullDatabaseBackupQueryHandler(IDatabaseBackupService databaseBackupService)
        {
            _databaseBackupService = databaseBackupService;
        }

        public async Task<Result<ExportResult>> Handle(ExportFullDatabaseBackupQuery request, CancellationToken cancellationToken)
        {
            var backupResult = await _databaseBackupService.CreateFullBackupSqlAsync(null, cancellationToken);
            if (backupResult.IsFailure)
                return Result.Failure<ExportResult>(backupResult.Error);

            var fileName = $"Naqlah_FullBackup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";
            var stream = backupResult.Value;
            return Result.Success(new ExportResult(stream, fileName, "application/sql"));
        }
    }
}

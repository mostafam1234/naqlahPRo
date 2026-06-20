using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.BackupFeature.Commands
{
    public sealed class RestoreFullDatabaseBackupCommand : IRequest<Result<DatabaseRestoreSummary>>
    {
        public Stream SqlStream { get; init; } = Stream.Null;
    }

    public sealed class RestoreFullDatabaseBackupCommandHandler
        : IRequestHandler<RestoreFullDatabaseBackupCommand, Result<DatabaseRestoreSummary>>
    {
        private readonly IDatabaseBackupService _databaseBackupService;

        public RestoreFullDatabaseBackupCommandHandler(IDatabaseBackupService databaseBackupService)
        {
            _databaseBackupService = databaseBackupService;
        }

        public Task<Result<DatabaseRestoreSummary>> Handle(RestoreFullDatabaseBackupCommand request, CancellationToken cancellationToken)
        {
            if (request.SqlStream == null || request.SqlStream == Stream.Null)
                return Task.FromResult(Result.Failure<DatabaseRestoreSummary>("BackupFileRequired"));

            return _databaseBackupService.RestoreMergeFromSqlAsync(request.SqlStream, null, cancellationToken);
        }
    }
}

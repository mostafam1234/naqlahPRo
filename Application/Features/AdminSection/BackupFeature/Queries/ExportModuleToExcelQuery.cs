using Application.Features.AdminSection.BackupFeature.Dtos;
using CSharpFunctionalExtensions;
using MediatR;
using System;

namespace Application.Features.AdminSection.BackupFeature.Queries
{
    public sealed record ExportModuleToExcelQuery : IRequest<Result<ExportResult>>
    {
        public string ModuleKey { get; init; } = string.Empty;
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public int LanguageId { get; init; } = 1;
    }
}

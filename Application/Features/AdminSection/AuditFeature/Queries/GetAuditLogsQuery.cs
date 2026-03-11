using Application.Features.AdminSection.AuditFeature.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.AuditFeature.Queries
{
    public sealed record GetAuditLogsQuery : IRequest<Result<PagedResult<AuditLogDto>>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 20;
        public int? UserId { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public string? ActionName { get; init; }
        public string? EntityType { get; init; }

        private class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<PagedResult<AuditLogDto>>>
        {
            private readonly INaqlahContext _context;

            public GetAuditLogsQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<PagedResult<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
            {
                var query = _context.AuditLogs
                    .Include(a => a.User)
                    .Include(a => a.Details)
                    .AsQueryable();

                if (request.UserId.HasValue)
                    query = query.Where(a => a.UserId == request.UserId.Value);

                if (request.FromDate.HasValue)
                {
                    var from = request.FromDate.Value.Date.ToUniversalTime();
                    query = query.Where(a => a.TimestampUtc >= from);
                }

                if (request.ToDate.HasValue)
                {
                    var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                    query = query.Where(a => a.TimestampUtc <= to);
                }

                if (!string.IsNullOrWhiteSpace(request.ActionName))
                    query = query.Where(a => a.ActionName.Contains(request.ActionName.Trim()));

                if (!string.IsNullOrWhiteSpace(request.EntityType))
                    query = query.Where(a => a.Details.Any(d => d.EntityType.Contains(request.EntityType.Trim())));

                var totalCount = await query.CountAsync(cancellationToken);

                var logs = await query
                    .OrderByDescending(a => a.TimestampUtc)
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(a => new AuditLogDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        UserName = a.User.UserName ?? string.Empty,
                        ActionName = a.ActionName,
                        TimestampUtc = a.TimestampUtc,
                        IpAddress = a.IpAddress,
                        UserAgent = a.UserAgent,
                        Details = a.Details.Select(d => new AuditLogDetailDto
                        {
                            Id = d.Id,
                            EntityType = d.EntityType,
                            EntityId = d.EntityId,
                            ChangeType = d.ChangeType,
                            OldValuesJson = d.OldValuesJson,
                            NewValuesJson = d.NewValuesJson
                        }).ToList()
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                return Result.Success(new PagedResult<AuditLogDto>
                {
                    Data = logs,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                });
            }
        }
    }
}

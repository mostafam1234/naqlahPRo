﻿using Application.Features.AdminSection.MainCategoryFeatures.Dtos;
using Application.Features.VehicleSection.Dtos;
using Application.Shared.Dtos;
using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.MainCategoryFeatures.Queries
{
    public sealed record GetAllMainCategoriesQueries: IRequest<PagedResult<MainCategoryAdminDto>>
    {
        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 10;
        public string? SearchTerm { get; init; }

        private class GetAllMainCategoriesQueriesHandler : IRequestHandler<GetAllMainCategoriesQueries, PagedResult<MainCategoryAdminDto>>
        {
            private readonly INaqlahContext _context;
            public GetAllMainCategoriesQueriesHandler(INaqlahContext context)
            {
                _context = context;
            }
            public async Task<PagedResult<MainCategoryAdminDto>> Handle(GetAllMainCategoriesQueries request, CancellationToken cancellationToken)
            {
                var query = _context.MainCategories.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = query.Where(x => x.ArabicName.Contains(request.SearchTerm) ||
                                           x.EnglishName.Contains(request.SearchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var categories = await query
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(x => new MainCategoryAdminDto
                    {
                        Id = x.Id,
                        ArabicName = x.ArabicName,
                        EnglishName = x.EnglishName,
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.Take);

                var pagedResult = new PagedResult<MainCategoryAdminDto>
                {
                    Data = categories,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                return pagedResult;
            }
        }

    }
}

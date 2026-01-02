using Application.Features.AdminSection.Dashboard.Dtos;
using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.Dashboard.Queries
{
    public sealed record GetTopCategoriesByOrderCountQuery(int LanguageId) : IRequest<Result<List<CategoryOrderCountDto>>>
    {
        private class GetTopCategoriesByOrderCountQueryHandler : IRequestHandler<GetTopCategoriesByOrderCountQuery, Result<List<CategoryOrderCountDto>>>
        {
            private readonly INaqlahContext _context;

            public GetTopCategoriesByOrderCountQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<List<CategoryOrderCountDto>>> Handle(GetTopCategoriesByOrderCountQuery request, CancellationToken cancellationToken)
            {
                var isArabic = request.LanguageId == (int)Language.Arabic;

                // Get all orders with their order details
                var ordersWithDetails = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ToListAsync(cancellationToken);

                // Flatten order details and group by category
                var categoryGroups = ordersWithDetails
                    .SelectMany(o => o.OrderDetails.Select(od => new
                    {
                        OrderId = o.Id,
                        MainCategoryId = od.MainCategoryId,
                        ArabicCategoryName = od.ArabicCategoryName,
                        EnglishCategoryName = od.EnglishCategoryName
                    }))
                    .GroupBy(x => new { x.MainCategoryId, x.ArabicCategoryName, x.EnglishCategoryName })
                    .Select(g => new CategoryOrderCountDto
                    {
                        MainCategoryId = g.Key.MainCategoryId,
                        CategoryName = isArabic ? g.Key.ArabicCategoryName : g.Key.EnglishCategoryName,
                        OrderCount = g.Select(x => x.OrderId).Distinct().Count()
                    })
                    .OrderByDescending(x => x.OrderCount)
                    .Take(4)
                    .ToList();

                return Result.Success(categoryGroups);
            }
        }
    }
}


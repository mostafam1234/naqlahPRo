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
    public sealed record GetMonthlyTopCategoriesQuery(int LanguageId) : IRequest<Result<List<MonthlyCategoryDataDto>>>
    {
        private class GetMonthlyTopCategoriesQueryHandler : IRequestHandler<GetMonthlyTopCategoriesQuery, Result<List<MonthlyCategoryDataDto>>>
        {
            private readonly INaqlahContext _context;

            public GetMonthlyTopCategoriesQueryHandler(INaqlahContext context)
            {
                _context = context;
            }

            public async Task<Result<List<MonthlyCategoryDataDto>>> Handle(GetMonthlyTopCategoriesQuery request, CancellationToken cancellationToken)
            {
                var isArabic = request.LanguageId == (int)Language.Arabic;
                var today = DateTime.Today;
                var last4Months = new List<DateTime>();

                // Get the last 4 months (including current month)
                for (int i = 0; i < 4; i++)
                {
                    var month = today.AddMonths(-i);
                    last4Months.Add(new DateTime(month.Year, month.Month, 1));
                }

                last4Months.Reverse(); // Reverse to get chronological order

                var monthNames = new Dictionary<int, Dictionary<int, string>>
                {
                    [(int)Language.Arabic] = new Dictionary<int, string>
                    {
                        { 1, "يناير" }, { 2, "فبراير" }, { 3, "مارس" }, { 4, "أبريل" },
                        { 5, "مايو" }, { 6, "يونيو" }, { 7, "يوليو" }, { 8, "أغسطس" },
                        { 9, "سبتمبر" }, { 10, "أكتوبر" }, { 11, "نوفمبر" }, { 12, "ديسمبر" }
                    },
                    [(int)Language.English] = new Dictionary<int, string>
                    {
                        { 1, "January" }, { 2, "February" }, { 3, "March" }, { 4, "April" },
                        { 5, "May" }, { 6, "June" }, { 7, "July" }, { 8, "August" },
                        { 9, "September" }, { 10, "October" }, { 11, "November" }, { 12, "December" }
                    }
                };

                // Get all orders with their order details and status histories
                var allOrders = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .Include(o => o.OrderStatusHistories)
                    .ToListAsync(cancellationToken);

                var result = new List<MonthlyCategoryDataDto>();

                foreach (var monthStart in last4Months)
                {
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    // Get orders created in this month using the earliest OrderStatusHistory entry
                    var ordersInMonth = allOrders
                        .Where(o => o.OrderStatusHistories.Any())
                        .Where(o =>
                        {
                            var earliestHistory = o.OrderStatusHistories.OrderBy(h => h.CreationDate).First();
                            return earliestHistory.CreationDate >= monthStart && earliestHistory.CreationDate <= monthEnd;
                        })
                        .Select(o => o.Id)
                        .Distinct()
                        .ToList();

                    // Get top 4 categories for orders in this month
                    var categoryGroups = allOrders
                        .Where(o => ordersInMonth.Contains(o.Id))
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

                    result.Add(new MonthlyCategoryDataDto
                    {
                        MonthName = monthNames[request.LanguageId][monthStart.Month],
                        MonthNumber = monthStart.Month,
                        Year = monthStart.Year,
                        Categories = categoryGroups
                    });
                }

                return Result.Success(result);
            }
        }
    }
}


using CSharpFunctionalExtensions;
using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AdminSection.NotificationFeature.Commands
{
    public sealed record SendCustomerNotificationCommand : IRequest<Result<int>>
    {
        public string ArabicTitle { get; init; } = string.Empty;
        public string EnglishTitle { get; init; } = string.Empty;
        public string ArabicMessage { get; init; } = string.Empty;
        public string EnglishMessage { get; init; } = string.Empty;
        public NotificationType NotificationType { get; init; } = NotificationType.CustomerGeneral;
        public bool TargetAll { get; init; } = true;
        public int? TargetCustomerType { get; init; }
        public List<int>? CustomerIds { get; init; }
        public bool IsScheduled { get; init; } = false;
        public DateTime? ScheduledDate { get; init; }

        private class Handler : IRequestHandler<SendCustomerNotificationCommand, Result<int>>
        {
            private readonly INaqlahContext _context;
            private readonly IDateTimeProvider _dateTimeProvider;
            private readonly INotificationService _fcmService;

            public Handler(INaqlahContext context, IDateTimeProvider dateTimeProvider, INotificationService fcmService)
            {
                _context = context;
                _dateTimeProvider = dateTimeProvider;
                _fcmService = fcmService;
            }

            public async Task<Result<int>> Handle(SendCustomerNotificationCommand request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(request.ArabicTitle) || string.IsNullOrWhiteSpace(request.EnglishTitle))
                    return Result.Failure<int>("العنوان مطلوب.");
                if (string.IsNullOrWhiteSpace(request.ArabicMessage) || string.IsNullOrWhiteSpace(request.EnglishMessage))
                    return Result.Failure<int>("نص الرسالة مطلوب.");

                var now = _dateTimeProvider.Now;

                if (request.IsScheduled)
                {
                    if (!request.ScheduledDate.HasValue)
                        return Result.Failure<int>("تاريخ الجدولة مطلوب.");
                    if (request.ScheduledDate.Value <= now)
                        return Result.Failure<int>("يجب أن يكون تاريخ الجدولة في المستقبل.");
                }

                if (!request.TargetAll && (request.CustomerIds == null || !request.CustomerIds.Any()))
                    return Result.Failure<int>("يجب تحديد العملاء المستهدفين أو اختيار إرسال للجميع.");

                // Fan-out now if:
                // - Immediate (any target type), OR
                // - Scheduled with specific customer IDs (records created now but hidden until ScheduledDate)
                bool fanOutNow = !request.IsScheduled || (!request.TargetAll && request.CustomerIds!.Any());
                bool isProcessed = fanOutNow;

                var notification = Notification.CreateForCustomers(
                    request.ArabicTitle, request.EnglishTitle,
                    request.ArabicMessage, request.EnglishMessage,
                    request.NotificationType, now,
                    request.TargetAll, request.TargetCustomerType,
                    request.IsScheduled, request.ScheduledDate,
                    isProcessed);

                var pushTokens = new List<string>();

                if (fanOutNow)
                {
                    IQueryable<Customer> customerQuery;

                    if (!request.TargetAll && request.CustomerIds!.Any())
                        customerQuery = _context.Customers.Where(c => request.CustomerIds.Contains(c.Id));
                    else
                    {
                        customerQuery = _context.Customers.AsQueryable();
                        if (request.TargetCustomerType.HasValue)
                            customerQuery = customerQuery.Where(c => (int)c.CustomerType == request.TargetCustomerType.Value);
                    }

                    var targetCustomers = await customerQuery
                        .Select(c => new { c.Id, c.AndriodDevice, c.IosDevice })
                        .ToListAsync(cancellationToken);

                    foreach (var customer in targetCustomers)
                        notification.AddCustomerNotification(customer.Id, now);

                    pushTokens = targetCustomers
                        .SelectMany(c => new[] { c.AndriodDevice, c.IosDevice })
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToList();
                }

                await _context.Notifications.AddAsync(notification, cancellationToken);
                var saveResult = await _context.SaveChangesAsyncWithResult();

                if (saveResult.IsFailure)
                    return Result.Failure<int>(saveResult.Error);

                if (pushTokens.Any())
                {
                    for (int i = 0; i < pushTokens.Count; i += 500)
                    {
                        var batch = pushTokens.Skip(i).Take(500).ToList();
                        await _fcmService.SendNotificationAsyncToMultipleDevices(new NotificationBodyForMultipleDevices
                        {
                            Title = request.ArabicTitle,
                            Body = request.ArabicMessage,
                            FireBaseTokens = batch,
                            PayLoad = new Dictionary<string, string>
                            {
                                ["notificationType"] = ((int)request.NotificationType).ToString()
                            }
                        });
                    }
                }

                return Result.Success(notification.Id);
            }
        }
    }
}

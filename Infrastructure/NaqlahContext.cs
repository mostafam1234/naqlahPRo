using Application.Shared.Services;
using CSharpFunctionalExtensions;
using Domain.DomainEventsHelper;
using Domain.Enums;
using Domain.InterFaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class NaqlahContext : IdentityDbContext<User,
                                                   Role,
                                                   int,
                                                   UserClaim,
                                                   UserRole,
                                                   UserLogin,
                                                   RoleClaims,
                                                   UserTokens>, INaqlahContext
    {
        private readonly IMediator mediator;
        private readonly IHangfireJobWriter hangfireJobWriter;
        private readonly IAuditScopeProvider auditScopeProvider;
        private readonly IAuditLogNotifier auditLogNotifier;

        public NaqlahContext(
            DbContextOptions<NaqlahContext> options,
            IMediator mediator,
            IHangfireJobWriter hangfireJobWriter,
            IAuditScopeProvider auditScopeProvider,
            IAuditLogNotifier auditLogNotifier) : base(options)
        {
            this.mediator = mediator;
            this.hangfireJobWriter = hangfireJobWriter;
            this.auditScopeProvider = auditScopeProvider;
            this.auditLogNotifier = auditLogNotifier;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NaqlahContext).Assembly);
        }

        public DatabaseFacade Database => base.Database;
        public DbSet<DeliveryMan> DeliveryMen { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<VehicleBrand> VehicleBrands { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<Resident> Residents { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Renter> Renters { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public DbSet<MainCategory> MainCategories { get; set; }
        public DbSet<Assistant> Assistants { get; set; }
        public DbSet<AssistanWork> AssistanWorks { get; set; }
        public DbSet<DeliveryVehicle> DeliveryVehicles { get; set; }
        public DbSet<DeliveryManLocation> DeliveryManLocations { get; set; }
        public DbSet<OrderPackage> OrderPackages { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Neighborhood> Neighborhoods { get; set; }
        public DbSet<WalletTransctions> WalletTransctions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Complain> Complains { get; set; }
        public DbSet<Suggestion> Suggestions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AuditLogDetail> AuditLogDetails { get; set; }
        public DbSet<DiscountCode> DiscountCodes { get; set; }


        public async Task<Result> SaveChangesAsyncWithResult()
        {

            try
            {
                await FirePreDomainEvents();

                // Capture changes before saving so we can read original values,
                // but persist the audit entries after the main SaveChanges so keys are available.
                var auditScope = auditScopeProvider.GetCurrentScope();
                var hasAuditScope = auditScope != null && auditScope.UserId > 0 && !string.IsNullOrWhiteSpace(auditScope.ActionName);

                List<AuditLogDetail> pendingDetails = new List<AuditLogDetail>();

                if (hasAuditScope)
                {
                    var entries = ChangeTracker.Entries()
                        .Where(e =>
                            e.Entity is not AuditLog &&
                            e.Entity is not AuditLogDetail &&
                            e.State != EntityState.Unchanged &&
                            e.State != EntityState.Detached)
                        .ToList();

                    foreach (var entry in entries)
                    {
                        var entityType = entry.Entity.GetType().Name;

                        var keyProperties = entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToList();
                        var entityId = keyProperties.Count == 0
                            ? string.Empty
                            : string.Join("|", keyProperties.Select(p => p.CurrentValue?.ToString() ?? string.Empty));

                        var changeType = entry.State switch
                        {
                            EntityState.Added => AuditChangeType.Insert,
                            EntityState.Modified => AuditChangeType.Update,
                            EntityState.Deleted => AuditChangeType.Delete,
                            _ => AuditChangeType.Update
                        };

                        string? oldValuesJson = null;
                        string? newValuesJson = null;

                        if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                        {
                            var originalValues = entry.Properties
                                .Where(p => !p.Metadata.IsPrimaryKey())
                                .ToDictionary(p => p.Metadata.Name, p => entry.OriginalValues[p.Metadata.Name]);

                            oldValuesJson = System.Text.Json.JsonSerializer.Serialize(originalValues);
                        }

                        if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
                        {
                            var currentValues = entry.Properties
                                .Where(p => !p.Metadata.IsPrimaryKey())
                                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

                            newValuesJson = System.Text.Json.JsonSerializer.Serialize(currentValues);
                        }

                        var detail = AuditLogDetail.Create(
                            entityType,
                            entityId,
                            changeType,
                            oldValuesJson,
                            newValuesJson);

                        pendingDetails.Add(detail);
                    }
                }

                var result = await base.SaveChangesAsync();

                if (hasAuditScope && pendingDetails.Count > 0)
                {
                    var auditLog = AuditLog.Create(
                        auditScope!.UserId,
                        auditScope.ActionName,
                        DateTime.UtcNow,
                        auditScope.IpAddress,
                        auditScope.UserAgent);

                    foreach (var detail in pendingDetails)
                    {
                        auditLog.AddDetail(detail);
                    }

                    await AuditLogs.AddAsync(auditLog);
                    await base.SaveChangesAsync();
                    await auditLogNotifier.NotifyNewAuditLogAsync(CancellationToken.None);
                }

                await FirePostDomainEvents();
                return Result.Success();
            }
            catch (Exception exp)
            {
                return Result.Failure(exp.Message);
            }
        }

        private async Task FirePostDomainEvents()
        {

            var domainEventsEnities = ChangeTracker.Entries<IEntity>()
                .Select(x => x.Entity)
                .Where(x => x.Events.Any(X => X.GetType().IsSubclassOf(typeof(PostEvent)))).ToList();

            foreach (var domianEntity in domainEventsEnities)
            {
                foreach (var @event in domianEntity.Events.Where(X => X.GetType().IsSubclassOf(typeof(PostEvent))))
                {
                    domianEntity.clearEventById(@event.EventId);
                    hangfireJobWriter.EnqueueBackGroundJob(@event);
                }
            }


        }


        private async Task FirePreDomainEvents()
        {

            var domainEventsEnities = ChangeTracker.Entries<IEntity>()
                .Select(x => x.Entity)
                .Where(x => x.Events.Any(x => x.GetType().IsSubclassOf(typeof(PreEvent)))).ToList();

            foreach (var domianEntity in domainEventsEnities)
            {
                foreach (var @event in domianEntity.Events.Where(x => x.GetType().IsSubclassOf(typeof(PreEvent))))
                {
                    domianEntity.clearEventById(@event.EventId);
                    await mediator.Publish(@event);
                }
            }


        }
    }
}

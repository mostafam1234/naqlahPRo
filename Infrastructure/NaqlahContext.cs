using CSharpFunctionalExtensions;
using Domain.DomainEventsHelper;
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

        public NaqlahContext(DbContextOptions<NaqlahContext> options,
                               IMediator mediator,
                               IHangfireJobWriter hangfireJobWriter) : base(options)
        {
            this.mediator = mediator;
            this.hangfireJobWriter = hangfireJobWriter;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NaqlahContext).Assembly);
        }

        public DatabaseFacade Database => base.Database;



        public async Task<Result> SaveChangesAsyncWithResult()
        {

            try
            {
                await FirePreDomainEvents();
                var result = await base.SaveChangesAsync();
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

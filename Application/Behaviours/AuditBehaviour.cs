using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.InterFaces;
using MediatR;

namespace Application.Behaviours
{
    public class AuditBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IAuditScopeProvider auditScopeProvider;
        private readonly IUserSession userSession;

        public AuditBehaviour(IAuditScopeProvider auditScopeProvider, IUserSession userSession)
        {
            this.auditScopeProvider = auditScopeProvider;
            this.userSession = userSession;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestType = request?.GetType();
            var ns = requestType?.Namespace ?? string.Empty;

            // Audit admin-side and admin-used feature sections (e.g. VehicleSection used by VehicleAdminController)
            var shouldAudit = ns.Contains("AdminSection", StringComparison.OrdinalIgnoreCase) ||
                              ns.Contains("VehicleSection", StringComparison.OrdinalIgnoreCase);

            if (!shouldAudit)
            {
                return await next();
            }

            try
            {
                var userId = userSession.UserId;
                var actionName = requestType!.Name;

                auditScopeProvider.SetScope(userId, actionName);

                var response = await next();
                return response;
            }
            finally
            {
                auditScopeProvider.ClearScope();
            }
        }
    }
}


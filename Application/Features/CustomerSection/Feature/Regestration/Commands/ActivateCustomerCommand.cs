using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Regestration.Commands
{
    public sealed record ActivateCustomerCommand:IRequest<Result>
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string ActiveCode { get; set; } = string.Empty;

        private class ActivateCustomerCommandHandler :
            IRequestHandler<ActivateCustomerCommand, Result>
        {
            private readonly INaqlahContext context;

            public ActivateCustomerCommandHandler(INaqlahContext context)
            {
                this.context = context;
            }
            public async Task<Result> Handle(ActivateCustomerCommand request, CancellationToken cancellationToken)
            {
                var user = await context.Users
                                      .AsTracking()
                                      .FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);

                if(user is null)
                {
                    return Result.Failure("User Not Found");
                }

                var result= user.ActivateUser(request.ActiveCode);
                if (result.IsFailure)
                {
                    return result;
                }

                var saveResult = await context.SaveChangesAsyncWithResult();
                return saveResult;
            }
        }
    }
}

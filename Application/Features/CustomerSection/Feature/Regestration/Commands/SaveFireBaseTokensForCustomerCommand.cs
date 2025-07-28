using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Regestration.Commands
{
    public class SaveFireBaseTokensForCustomerCommand:IRequest<Result>
    {
        public string AndroidDevice { get; set; } = string.Empty;
        public string IosDevice { get; set; } = string.Empty;

        private class SaveFireBaseTokensForCustomerCommandHandler :
            IRequestHandler<SaveFireBaseTokensForCustomerCommand, Result>
        {
            private readonly INaqlahContext context;
            private readonly IUserSession userSession;

            public SaveFireBaseTokensForCustomerCommandHandler(INaqlahContext context,
                                                               IUserSession userSession)
            {
                this.context = context;
                this.userSession = userSession;
            }
            public async Task<Result> Handle(SaveFireBaseTokensForCustomerCommand request, CancellationToken cancellationToken)
            {
                var user = await context.Customers
                                      .AsTracking()
                                      .FirstOrDefaultAsync(x =>x.UserId==userSession.UserId);

                if (user is null)
                {
                    return Result.Failure("User Not Found");
                }

                user.AddFireBaseDevices(request.AndroidDevice, request.IosDevice);
                var saveResult = await context.SaveChangesAsyncWithResult();
                return saveResult;
            }
        }
    }
}

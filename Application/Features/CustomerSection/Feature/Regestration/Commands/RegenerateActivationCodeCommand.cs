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
    public sealed record RegenerateActivationCodeCommand:IRequest<Result>
    {
        public string PhoneNumber { get; set; } = string.Empty;

        private class RegenerateActivationCodeCommandHandler :
            IRequestHandler<RegenerateActivationCodeCommand, Result>
        {
            private readonly INaqlahContext context;

            public RegenerateActivationCodeCommandHandler(INaqlahContext context)
            {
                this.context = context;
            }
            public async Task<Result> Handle(RegenerateActivationCodeCommand request, CancellationToken cancellationToken)
            {
                var user = await context.Users
                                      .AsTracking()
                                      .FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);

                if (user == null)
                {
                    return Result.Failure("Invalid User Name");
                }

                user.GenerateActivationCode();
                var saveResult = await context.SaveChangesAsyncWithResult();
                return saveResult;
            }
        }
    }
}

using CSharpFunctionalExtensions;
using MediatR;

namespace Application.Features.CustomerSection.Feature.CustomerInfo.Queries
{
    public class GetCustomerInfoQuery : IRequest<Result<Dtos.CustomerInfoDto>>
    {

    }
}
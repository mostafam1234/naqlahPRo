using Application.Features.DeliveryManSection.CurrentDeliveryMen.Dtos;
using CSharpFunctionalExtensions;
using MediatR;

namespace Application.Features.DeliveryManSection.CurrentDeliveryMen.Commands
{
    public class AddDeliveryManCommand : IRequest<Result<int>>
    {
        public AddDeliveryManDto DeliveryMan { get; set; }
    }
}
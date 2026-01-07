using Application.Features.DeliveryManSection.CurrentDeliveryMen.Dtos;
using CSharpFunctionalExtensions;
using MediatR;

namespace Application.Features.DeliveryManSection.CurrentDeliveryMen.Commands
{
    public class UpdateDeliveryManCommand : IRequest<Result<int>>
    {
        public int DeliveryManId { get; set; }
        public AddDeliveryManDto DeliveryMan { get; set; }
    }
}


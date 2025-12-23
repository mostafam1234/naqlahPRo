using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.GoogleMap
{
    public interface IGoogleMapService
    {
        Task<GoogleResponse> CalculateOrderDeliveryTime(LocationPoint orgin,
                                                                     List<LocationPoint> wayPoints,
                                                                     LocationPoint destenation);
    }
}

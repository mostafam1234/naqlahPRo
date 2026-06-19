using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.CustomerSection.Feature.Order.Dtos
{
    public class SelectVehicleTypeDto
    {
        public int OrderId { get; set; }
        public int VehicleTypeId { get; set; }

        /// <summary>
        /// Selected payment method (backend enum: Cash/COD = 1, Online/Mada = 2, Wallet = 3).
        /// Stored on the order so payment-timing logic can decide when to charge.
        /// Defaults to Wallet when not supplied to preserve existing behavior.
        /// </summary>
        public int? PaymentMethodId { get; set; }
    }

    public class SelectVehicleTypeResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int NotifiedDeliveryMenCount { get; set; }
    }
}
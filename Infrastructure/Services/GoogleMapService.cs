using Application.Services.GoogleMap;
using Domain.InterFaces;
using Domain.Shared;
using GoogleMapsApi;
using GoogleMapsApi.Entities.Common;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using GoogleMapsApi.Entities.DistanceMatrix.Request;
using GoogleMapsApi.Entities.DistanceMatrix.Response;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalabatkData.GoogleMapServices
{
    public class GoogleMapService : IGoogleMapService
    {
        private readonly IReadFromAppSetting config;
        private readonly ILogger<GoogleMapService> logger;

        public GoogleMapService(IReadFromAppSetting config,
                                ILogger<GoogleMapService> logger)
        {
            this.config = config;
            this.logger = logger;
        }
        public async Task<GoogleResponse> CalculateOrderDeliveryTime(LocationPoint orgin,
                                                                     List<LocationPoint> wayPoints,
                                                                     LocationPoint destenation)
        {
            try
            {
                var googleApiKey = config.GetValue<string>("GoogleApiKey");
               
                var destination = new Location(destenation.Latitude,
                                               destenation.Longitude).LocationString;

                var orginLocation = new Location(orgin.Latitude,
                                                 orgin.Longitude).LocationString;


                var googleWayPoints = wayPoints.Select(x => new Location(x.Latitude, x.Longitude).LocationString).ToArray();



                var directionRequest = new DirectionsRequest
                {
                    Origin = orginLocation,
                    Destination = destination,
                    ApiKey = googleApiKey,
                    TravelMode = TravelMode.Driving,
                    DepartureTime = DateTime.Now,
                    Waypoints = googleWayPoints,
                    OptimizeWaypoints = true
                };

                var response = await GoogleMaps.Directions.QueryAsync(directionRequest);


                if (response.Status != DirectionsStatusCodes.OK)
                {
                    return new GoogleResponse
                    {
                        DeliveryTime = 0,
                        EncodedPoints = ""
                    };
                }

                if (!response.Routes.Any())
                {
                    return new GoogleResponse
                    {
                        DeliveryTime = 0,
                        EncodedPoints = ""
                    };
                }


                var route = response.Routes.First();

                if (route is null)
                {
                    return new GoogleResponse
                    {
                        DeliveryTime = 0,
                        EncodedPoints = ""
                    };
                }


                var path = route.OverviewPath?.GetRawPointsData() ?? "";

                TimeSpan totalDuration = TimeSpan.Zero;
                double totalDistanceInMeters = 0;
                foreach (var leg in route.Legs)
                {
                    totalDuration = totalDuration.Add(leg.DurationInTraffic == null ? leg.Duration.Value : leg.DurationInTraffic.Value);

                    totalDistanceInMeters += leg.Distance.Value;
                }

                var totalDurationInMunites = totalDuration.TotalMinutes;
                var totalDistanceInKm = totalDistanceInMeters / 1000.0;
                return new GoogleResponse
                {
                    DeliveryTime = totalDurationInMunites,
                    EncodedPoints = path,
                    DistanceInKiloMeter = totalDistanceInKm
                };

            }




            catch (Exception ex)
            {
                logger.LogError(ex, "An Error Occur In Google Map Apis");
                return new GoogleResponse
                {
                    DeliveryTime = 0,
                    EncodedPoints = ""
                };
            }

        }
        public async Task<bool> IsWithinRadius(double lat1, double lng1, double lat2, double lng2, int radiusMeters = 200)
        {
            var googleApiKey = config.GetValue<string>("GoogleApiKey");

            var request = new DistanceMatrixRequest
            {
                Origins = new[] { $"{lat1},{lng1}" },
                Destinations = new[] { $"{lat2},{lng2}" },
                ApiKey = googleApiKey,
                Mode = DistanceMatrixTravelModes.driving
            };

            var response = await GoogleMaps.DistanceMatrix.QueryAsync(request);

            if (response.Status != DistanceMatrixStatusCodes.OK)
                return false;

            var distance = response.Rows.First().Elements.First().Distance.Value; // in meters

            return distance <= radiusMeters;
        }
    }
}

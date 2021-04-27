using Newtonsoft.Json;
using System;

namespace BlazorApp.Client.Models
{

    public class BaseObject
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }

    public class Driver : BaseObject
    {
        [JsonProperty("driverId")]
        public string DriverId { get; set; }
    }

    public class Icream : BaseObject
    {
        [JsonProperty("icecreamId")]
        public int IcecreamId { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class OrderBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("icecream")]
        public Icream Icecream { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("driver")]
        public Driver Driver { get; set; }
        [JsonProperty("fullAddress")]
        public string FullAddress { get; set; }

    }

    public class Order : OrderBase
    {
        [JsonProperty("deliveryPosition")]
        public MyPoint DeliveryPosition { get; set; }
        [JsonProperty("lastPosition")]
        public MyPoint LastPosition { get; set; }

        static double ToRadians(
           double angleIn10thofaDegree)
        {
            // Angle in 10th
            // of a degree
            return (angleIn10thofaDegree *
                           Math.PI) / 180;
        }

        public double Distance
        {
            get
            {
                {


                    // The math module contains
                    // a function named toRadians
                    // which converts from degrees
                    // to radians.
                    var lon1 = ToRadians(DeliveryPosition.Longitude);
                    var lon2 = ToRadians(LastPosition.Longitude);
                    var lat1 = ToRadians(DeliveryPosition.Latitude);
                    var lat2 = ToRadians(LastPosition.Latitude);

                    // Haversine formula
                    double dlon = lon2 - lon1;
                    double dlat = lat2 - lat1;
                    double a = Math.Pow(Math.Sin(dlat / 2), 2) +
                               Math.Cos(lat1) * Math.Cos(lat2) *
                               Math.Pow(Math.Sin(dlon / 2), 2);

                    double c = 2 * Math.Asin(Math.Sqrt(a));

                    // Radius of earth in
                    // kilometers. Use 3956
                    // for miles
                    double r = 6371;

                    // calculate the result
                    return (c * r);
                }
            }
        }

    }

    public class MyPoint
    {
        public MyPoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        [JsonProperty("latitude")]
        public double Latitude { get; }
        [JsonProperty("longitude")]
        public double Longitude { get; }
    }
}

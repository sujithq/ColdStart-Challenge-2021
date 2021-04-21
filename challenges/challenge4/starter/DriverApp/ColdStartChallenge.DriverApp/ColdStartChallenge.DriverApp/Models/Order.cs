using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Point = Microsoft.Azure.Cosmos.Spatial.Point;


namespace ColdStartChallenge.DriverApp.Models
{
    //public class Order
    //{
    //    public int Id { get; set; }
    //    public string User { get; set; }
    //    public DateTime Date { get; set; }
    //    public int IcecreamId { get; set; }
    //    public OrderStatus Status { get; set; }
    //    public int? DriverId { get; set; }
    //    public string FullAddress { get; set; }
    //    public Location LastPosition { get; set; }
    //    public string Name { get; set; }
    //    public string ImageUrl { get; set; }
    //    public string Description { get; set; }
    //    public Location Location { get; set; } //Nick should be adding this to the API
    //}

    public partial class Order
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("icecream")]
        public Icecream Icecream { get; set; }

        [JsonProperty("status")]
        private string Status { get; set; }

        [JsonIgnore]
        public OrderStatus OrderStatus
        {
            get => Enum<OrderStatus>.Parse(Status);
            set => Status = value.ToString();
        }

        [JsonProperty("driver")]
        public Driver Driver { get; set; }

        [JsonProperty("fullAddress")]
        public string FullAddress { get; set; }

        [JsonProperty("deliveryPosition")]
        private Point DeliveryPosition { get; set; }

        [JsonIgnore]
        public Location Location
        {
            get
            {
                return DeliveryPosition != null ? new Location() { Latitude = DeliveryPosition.Position.Latitude, Longitude = DeliveryPosition.Position.Longitude } : null;
            }
        }

        [JsonProperty("lastPosition")]
        private Point LastPosition { get; set; }

        [JsonIgnore]
        public Location DriverLocation
        {
            get
            {
                return LastPosition != null ? new Location() { Latitude = LastPosition.Position.Latitude, Longitude = LastPosition.Position.Longitude } : null;
            }
            set
            {
                if (value != null)
                    LastPosition = new Point(value.Longitude, value.Latitude);
                else
                    LastPosition = null;
            }
        }

        [JsonIgnore]
        public string Name
        {
            get => Id.ToString();
        }
    }

}

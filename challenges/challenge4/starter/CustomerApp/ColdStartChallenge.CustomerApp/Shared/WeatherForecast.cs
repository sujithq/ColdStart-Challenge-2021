using Newtonsoft.Json;
using System;

namespace BlazorApp.Shared
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public string Summary { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    public class MessageCtx
    {
        public string User { get; set; }
        public string Message { get; set; }
    }

    //public class Order
    //{
    //    public string id { get; set; }
    //    public string user { get; set; }
    //    public DateTime date { get; set; }
    //    public IceCream icecream { get; set; }
    //    public string status { get; set; }

    //    public Driver driver { get; set; }

    //    public string fullAddress { get; set; }

    //    public Position deliveryPosition { get; set; }
    //    public Position lastPosition { get; set; }

    //}

    //public class IceCream: CommonProps
    //{
    //    public string icecreamId { get; set; }
    //    public string description { get; set; }
    //}

    //public class Driver: CommonProps
    //{
    //    public string driverId { get; set; }
    //}

    //public class CommonProps
    //{
    //    public string name { get; set; }
    //    public string imageUrl { get; set; }

    //}

    //public class Position
    //{
    //    public string type { get; set; }
    //    public double[] coordinates { get; set; }
    //}
}

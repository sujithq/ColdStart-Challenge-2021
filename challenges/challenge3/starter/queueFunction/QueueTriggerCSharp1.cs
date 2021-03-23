using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Company.Function
{
    public static class QueueTriggerCSharp1
    {
        [Function("QueueTriggerCSharp1")]
        [CosmosDBOutput(databaseName: "coldstartchallenge2021",
                    collectionName: "orders",
                    ConnectionStringSetting = "CosmosDBConnection")]
        public static OrderDB Run([QueueTrigger("preorder", Connection = "sujithqcschallenge2021_STORAGE")] string myQueueItem,
                FunctionContext context)
        {
            var orderIn = JsonConvert.DeserializeObject<Order>(myQueueItem);

            // Get Icecream Info
            var client = new RestClient(Environment.GetEnvironmentVariable("ApiBase"));
            client.UseJson();

            var request = new RestRequest($"{Environment.GetEnvironmentVariable("ApiBase")}/catalog/{orderIn.IcecreamId}");

            var lst = client.Get<List<Catalog>>(request);

            var icecreams = JsonConvert.DeserializeObject<List<Catalog>>(lst.Content);

            var icecream = icecreams.First();

            var document = new OrderDB
            {
                Id = orderIn.Id,
                User = orderIn.User,
                Date = orderIn.Date,
                Icecream = new Icream()
                {
                    IcecreamId = orderIn.IcecreamId,
                    Name = icecream.Name,
                    Description = icecream.Description,
                    ImageUrl = icecream.ImageUrl
                },
                Status = "Accepted",
                Driver = new Driver()
                {
                    DriverId = Guid.Empty,
                    Name = null,
                    ImageUrl = null
                },
                FullAddress = orderIn.FullAddress,
                DeliveryPosition = null,
                LastPosition = null
            };
            return document;
        }

        [Function("TimerTriggerCSharp")]
        public static void RunTimer([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, FunctionContext context)
        {
            var logger = context.GetLogger("TimerTriggerCSharp");
            if (myTimer.IsPastDue)
            {
                logger.LogInformation("Timer is running late!");
            }
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }

    public class TimerInfo
    {
        /// <summary>
        /// Gets the current schedule status for this timer.
        /// If schedule monitoring is not enabled for this timer (see <see cref="TimerTriggerAttribute.UseMonitor"/>)
        /// this property will return null.
        /// </summary>
        public ScheduleStatus? ScheduleStatus { get; set; }

        /// <summary>
        /// Gets a value indicating whether this timer invocation
        /// is due to a missed schedule occurrence.
        /// </summary>
        public bool IsPastDue { get; set; }
    }

    public class ScheduleStatus
    {
        /// <summary>
        /// Gets or sets the last recorded schedule occurrence.
        /// </summary>
        public DateTime Last { get; set; }

        /// <summary>
        /// Gets or sets the expected next schedule occurrence.
        /// </summary>
        public DateTime Next { get; set; }

        /// <summary>
        /// Gets or sets the last time this record was updated. This is used to re-calculate Next
        /// with the current Schedule after a host restart.
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    public class Driver
    {
        [JsonProperty(PropertyName = "driverId")]
        public Guid DriverId { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; set; }
    }

    public class Icream
    {
        [JsonProperty(PropertyName = "icecreamId")]
        public int IcecreamId { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; set; }
    }

    public class OrderDB
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }
        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }
        [JsonProperty(PropertyName = "icecream")]
        public Icream Icecream { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "driver")]
        public Driver Driver { get; set; }
        [JsonProperty(PropertyName = "fullAddress")]
        public string FullAddress { get; set; }
        [JsonProperty(PropertyName = "deliveryPosition")]
        public string DeliveryPosition { get; set; }
        [JsonProperty(PropertyName = "lastPosition")]
        public string LastPosition { get; set; }

    }

    public class Order
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string User { get; set; }
        public int IcecreamId { get; set; }
        public string Status { get; set; }
        public string DriverId { get; set; }
        public string FullAddress { get; set; }
        public string LastPosition { get; set; }
    }

    public class Catalog{
      public int Id { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public string ImageUrl { get; set; }
    
    }
}

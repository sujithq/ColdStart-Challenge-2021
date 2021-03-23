using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
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
            var logger = context.GetLogger("QueueTriggerCSharp1");

            logger.LogInformation($"C# Queue trigger function processing: {myQueueItem}");

            logger.LogInformation($"C# Queue trigger function Deserialize: {myQueueItem}");

            var orderIn = JsonConvert.DeserializeObject<Order>(myQueueItem);

            logger.LogInformation($"C# Queue trigger function Composing: {myQueueItem}");

            // Get Icream Info
            var client = new RestClient(Environment.GetEnvironmentVariable("ApiBase"));
            var request = new RestRequest("catalog/{id}")
              .AddUrlSegment("id", orderIn.IcecreamId);

            var lst = client.Get<List<Catalog>>(request);

            var icecream = lst.Data.First();

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
            logger.LogInformation($"C# Queue trigger function Processed: {document}");

            return document;
        }
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

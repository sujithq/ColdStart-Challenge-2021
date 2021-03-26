using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;
using Microsoft.Azure.Cosmos;
using System.IO;
using System.Text;
using Newtonsoft.Json.Serialization;
using Microsoft.Azure.Cosmos.Spatial;
using BingMapsRESTToolkit;
using Point = Microsoft.Azure.Cosmos.Spatial.Point;

namespace Company.Function
{
    public static class QueueTriggerCSharp1
    {

        private static readonly JsonSerializer Serializer = new JsonSerializer();

        [Function("QueueTriggerCSharp1")]
        [CosmosDBOutput(databaseName: "%CosmosDBdatabaseName%",
                    collectionName: "%CosmosDBcollectionName%",
                    ConnectionStringSetting = "CosmosDBConnection")]
        public static string Run([QueueTrigger("%QueueName%", Connection = "sujithqcschallenge2021_STORAGE")] string myQueueItem,
                FunctionContext context)
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

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
                    DriverId = null,
                    Name = null,
                    ImageUrl = null
                },
                FullAddress = orderIn.FullAddress,
                DeliveryPosition = null,
                LastPosition = null
            };
            return JsonConvert.SerializeObject(document, new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });
        }

        [Function("TimerTriggerCSharp")]
        public static async Task RunTimer([TimerTrigger("%Schedule%")] TimerInfo myTimer, FunctionContext context)
        {
            var logger = context.GetLogger("TimerTriggerCSharp");
            if (myTimer.IsPastDue)
            {
                logger.LogInformation("Timer is running late!");
            }

            using (CosmosClient cosmosClient = new CosmosClient(Environment.GetEnvironmentVariable("CosmosDBep"), Environment.GetEnvironmentVariable("CosmosDBkey")))
            {
                Container container = cosmosClient.GetContainer(Environment.GetEnvironmentVariable("CosmosDBdatabaseName"), Environment.GetEnvironmentVariable("CosmosDBcollectionName"));

                QueryDefinition query = new QueryDefinition(
                                "select * from orders s where s.status = @StatusInput ")
                                .WithParameter("@StatusInput", "Accepted");

                List<OrderDB> allAcceptedOrders = new List<OrderDB>();
                using (FeedIterator<OrderDB> resultSet = container.GetItemQueryIterator<OrderDB>(
                    query,
                    requestOptions: new QueryRequestOptions()
                    {
                        MaxItemCount = 1
                    }))
                {
                    while (resultSet.HasMoreResults)
                    {
                        FeedResponse<OrderDB> response = await resultSet.ReadNextAsync();
                        //if (response.Any())
                        //{
                        //    OrderDB order = response.First();
                        //    Console.WriteLine($"Order Status: {order.status}; Id: {order.id};");
                        //}
                        if (response.Diagnostics != null)
                        {
                            Console.WriteLine($" Diagnostics {response.Diagnostics.ToString()}");
                        }
                        allAcceptedOrders.AddRange(response);
                    }
                }

                foreach (var order in allAcceptedOrders)
                {
                    Console.WriteLine($"Order Status: {order.Status}; Id: {order.Id}; User: {order.User};");
                    order.Status = "Ready";

                    order.DeliveryPosition = await GetLocation(order.FullAddress);

                    ItemResponse<OrderDB> response = await container.ReplaceItemAsync(
                        partitionKey: new PartitionKey(order.User),
                        id: order.Id,
                        item: order);

                    Console.WriteLine($"New Order Status: {order.Status}; Id: {order.Id}; User: {order.User};");

                    if (response.Diagnostics != null)
                    {
                        Console.WriteLine($" Diagnostics {response.Diagnostics.ToString()}");
                    }
                }
            }
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        static async Task<Point> GetLocation(string address)
        {
            //Create a request.
            var request = new GeocodeRequest()
            {
                Query = address,
                IncludeIso2 = true,
                IncludeNeighborhood = true,
                MaxResults = 25,
                BingMapsKey = Environment.GetEnvironmentVariable("BingMapsKey")
            };

            //Process the request by using the ServiceManager.
            var response = await request.Execute();

            if (response != null &&
                response.ResourceSets != null &&
                response.ResourceSets.Length > 0 &&
                response.ResourceSets[0].Resources != null &&
                response.ResourceSets[0].Resources.Length > 0)
            {
                var result = response.ResourceSets[0].Resources[0] as BingMapsRESTToolkit.Location;

                return new Point(result.Point.Coordinates[0], result.Point.Coordinates[1]);
            }

            return new Point(NextDoubleLinear(-180, 180), NextDoubleLinear(-180, 180));
        }

        public static double NextDoubleLinear(double minValue, double maxValue)
        {
            Random random = new Random();
            double sample = random.NextDouble();
            return (maxValue * sample) + (minValue * (1d - sample));
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
        [JsonProperty("driverId")]
        public string DriverId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }

    public class Icream
    {
        [JsonProperty("icecreamId")] 
        public int IcecreamId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }

    public class OrderDB
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
        [JsonProperty("deliveryPosition")]
        public Point DeliveryPosition { get; set; }
        [JsonProperty("lastPosition")]
        public Point LastPosition { get; set; }

    }

    public class Order
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string User { get; set; }
        public int IcecreamId { get; set; }
        public string Status { get; set; }
        public string DriverId { get; set; }
        public string FullAddress { get; set; }
        public string LastPosition { get; set; }
    }

    public class Catalog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

    }
}

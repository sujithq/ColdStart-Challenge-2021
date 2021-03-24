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

namespace Company.Function
{
    public static class QueueTriggerCSharp1
    {

        private static readonly JsonSerializer Serializer = new JsonSerializer();

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
                id = orderIn.Id,
                user = orderIn.User,
                date = orderIn.Date,
                icecream = new Icream()
                {
                    icecreamId = orderIn.IcecreamId,
                    name = icecream.Name,
                    description = icecream.Description,
                    imageUrl = icecream.ImageUrl
                },
                status = "Accepted",
                driver = new Driver()
                {
                    driverId = null,
                    name = null,
                    imageUrl = null
                },
                fullAddress = orderIn.FullAddress,
                deliveryPosition = null,
                lastPosition = null
            };
            return document;
        }

        [Function("TimerTriggerCSharp")]
        public static async Task RunTimer([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, FunctionContext context)
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
                    Console.WriteLine($"Order Status: {order.status}; Id: {order.id}; User: {order.user};");
                    order.status = "Ready";
                    Console.WriteLine($"New Order Status: {order.status}; Id: {order.id}; User: {order.user};");

                    ItemResponse<OrderDB> response = await container.ReplaceItemAsync(
                        partitionKey: new PartitionKey(order.user),
                        id: order.id,
                        item: order);

                    if (response.Diagnostics != null)
                    {
                        Console.WriteLine($" Diagnostics {response.Diagnostics.ToString()}");
                    }
                }
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
        public string driverId { get; set; }
        public string name { get; set; }
        public string imageUrl { get; set; }
    }

    public class Icream
    {
        public int icecreamId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string imageUrl { get; set; }
    }

    public class OrderDB
    {
        public string id { get; set; }
        public string user { get; set; }
        public DateTime date { get; set; }
        public Icream icecream { get; set; }
        public string status { get; set; }
        public Driver driver { get; set; }
        public string fullAddress { get; set; }
        public string deliveryPosition { get; set; }
        public string lastPosition { get; set; }

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

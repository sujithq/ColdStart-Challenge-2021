using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Spatial;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BlazorApp.Api
{
    public static class WeatherForecastFunction
    {
        private static string GetSummary(int temp)
        {
            var summary = "Mild";

            if (temp >= 32)
            {
                summary = "Hot";
            }
            else if (temp <= 16 && temp > 0)
            {
                summary = "Cold";
            }
            else if (temp <= 0)
            {
                summary = "Freezing";
            }

            return summary;
        }

        [FunctionName("WeatherForecast")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var randomNumber = new Random();
            var temp = 0;

            var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = temp = randomNumber.Next(-20, 55),
                Summary = GetSummary(temp)
            }).ToArray();

            return new OkObjectResult(result);
        }

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "chat")] SignalRConnectionInfo connectionInfo, ILogger log)
        {
            log.LogInformation($"negotiate - url: {connectionInfo.Url} - at: {connectionInfo.AccessToken}");
            return connectionInfo;
        }

        [FunctionName("SendMessage")]
        public static async Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages, ILogger log)
        {

            string requestBody = string.Empty;
            using (StreamReader streamReader =  new  StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            var data = JsonConvert.DeserializeObject<MessageCtx>(requestBody);

            log.LogInformation($"SendMessage - User: {data.user} - Msg: {data.message}");
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "ReceiveMessage",
                    Arguments = new[] { data.user, data.message }
                });
        }

        [FunctionName("SendOrderUpdate")]
        public static async Task SendOrderUpdate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages, ILogger log)
        {

            string requestBody = string.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            var data = JsonConvert.DeserializeObject<OrderCtx>(requestBody);

            log.LogInformation($"SendOrderUpdate - User: {data.user} - OrderId: {data.orderId} - Latitude: {data.latitude} - Longitude: {data.longitude}");
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "ReceiveOrderUpdate",
                    Arguments = new[] { data.user, data.orderId, data.latitude.ToString(), data.longitude.ToString() }
                });
        }



        [FunctionName("MyOrders")]
        public static async Task<List<Order>> GetMyOrders(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,ILogger log)
        {


            var user = StaticWebAppsAuth.Parse(req);

            using CosmosClient cosmosClient = new CosmosClient(Environment.GetEnvironmentVariable("CosmosDBep"), Environment.GetEnvironmentVariable("CosmosDBkey"));
            Container container = cosmosClient.GetContainer(Environment.GetEnvironmentVariable("CosmosDBdatabaseName"), Environment.GetEnvironmentVariable("CosmosDBcollectionName"));

            QueryDefinition query = new QueryDefinition(
                            "select * from orders s where s.status = @StatusInput and s.user = @UserInput")
                            .WithParameter("@StatusInput", "Delivering")
                            .WithParameter("@UserInput", user.FindFirstValue(ClaimTypes.Name));

            List<Order> allAcceptedOrders = new List<Order>();

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
                    if (response.Any())
                    {
                        OrderDB order = response.First();
                        allAcceptedOrders.Add(new Order(order));
                    }
                    if (response.Diagnostics != null)
                    {
                        Console.WriteLine($" Diagnostics {response.Diagnostics.ToString()}");
                    }
                }
            }

            return allAcceptedOrders;

        }


    }

    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public string Summary { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    public class MessageCtx{
        public string user { get; set; }
        public string message { get; set; }
    }

    public class OrderCtx
    {
        public string user { get; set; }
        public string orderId { get; set; }

        public double latitude { get; set; }
        public double longitude { get; set; }
    }


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

    public class OrderDB:OrderBase
    {
        [JsonProperty("deliveryPosition")]
        public Point DeliveryPosition { get; set; }
        [JsonProperty("lastPosition")]
        public Point LastPosition { get; set; }
    }

    public class Order : OrderBase
    {
        [JsonProperty("deliveryPosition")]
        public MyPoint DeliveryPosition { get; set; }
        [JsonProperty("lastPosition")]
        public MyPoint LastPosition { get; set; }


        public Order(OrderDB order)
        {
            Id = order.Id;
            User = order.User;
            Date = order.Date;
            Icecream = order.Icecream;
            Status = order.Status;
            Driver = order.Driver;
            FullAddress = order.FullAddress;
            DeliveryPosition = new MyPoint(order.DeliveryPosition.Position.Latitude, order.DeliveryPosition.Position.Longitude);
            LastPosition = new MyPoint(order.LastPosition.Position.Latitude, order.LastPosition.Position.Longitude);
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


    public static class StaticWebAppsAuth
    {
        private class ClientPrincipal
        {
            public string IdentityProvider { get; set; }
            public string UserId { get; set; }
            public string UserDetails { get; set; }
            public IEnumerable<string> UserRoles { get; set; }
        }

        public static ClaimsPrincipal Parse(HttpRequest req)
        {
            var principal = new ClientPrincipal();

            if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
            {
                var data = header[0];
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.ASCII.GetString(decoded);
                principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            principal.UserRoles = principal.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

            if (!principal.UserRoles?.Any() ?? true)
            {
                return new ClaimsPrincipal();
            }

            var identity = new ClaimsIdentity(principal.IdentityProvider);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
            identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
            identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

            return new ClaimsPrincipal(identity);
        }
    }
}

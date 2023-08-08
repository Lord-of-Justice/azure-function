using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using Microsoft.Azure.Cosmos;

namespace OrderItemsReserver
{
    public static class OrderItemsReserver
    {
        private static readonly string EndpointUri = Environment.GetEnvironmentVariable("EndPointUri");
        private static readonly string PrimaryKey = Environment.GetEnvironmentVariable("PrimaryKey");
        private static readonly string databaseId = "orders";
        private static readonly string containerId = "Items";

        [FunctionName("OrderItemsReserver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonConvert.DeserializeObject<Order>(requestBody);

            try
            {
                var cosmosClient = new CosmosClient(
                    EndpointUri, 
                    PrimaryKey, 
                    new CosmosClientOptions());

                // Get db
                var dbResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
                var db = dbResponse.Database;
                Console.WriteLine("Get Database: {0}\n", db.Id);

                // Get container
                var containerResponse = await db.CreateContainerIfNotExistsAsync(containerId, "/buyerId");
                var container = containerResponse.Container;
                Console.WriteLine("Get Container: {0}\n", container.Id);

                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<Order> orderResponse = await container.CreateItemAsync<Order>(order, new PartitionKey(order.BuyerId));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", orderResponse.Resource.Id, orderResponse.RequestCharge);

            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                log.LogInformation($"{de.StatusCode} error occurred: {de}");
            }
            catch (Exception e)
            {
                log.LogInformation($"error: {e.Message}");
            }


            log.LogInformation($"OrderItemsReserver HTTP trigger successfully processed for order {order.Id}");

            return new OkObjectResult("Func successfully processed");
        }
    }
}

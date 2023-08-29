using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderItemReserver.ServiceBusTrigger;

public class OrderItemsReserver
{
    private const string queueName = "orders";
    private const string serviceBusConnectionString = "Endpoint=sb://orderitemsreservermessages.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=fPo/UpC+7viFmunF44uNSXdvaNxS04FIw+ASbPBIXvg=";

    [FunctionName("OrderItemsReserver")]
    public void Run([ServiceBusTrigger(queueName, Connection = serviceBusConnectionString)] string myQueueItem, ILogger log)
    {
        log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        
        var order = JsonConvert.DeserializeObject<Order>(myQueueItem);

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

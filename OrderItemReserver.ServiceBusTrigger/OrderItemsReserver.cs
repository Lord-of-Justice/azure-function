using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace OrderItemReserver.ServiceBusTrigger;

public class OrderItemsReserver
{
    private static string logicAppUri = @"https://prod-78.westeurope.logic.azure.com:443/workflows/46fff900abf94eb38e0320703bdd9ee4/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=1boDcqAvO-1HXNfAP-auH5r_BflsTUguZbUDr_eoOrg";
    private static HttpClient httpClient = new HttpClient();

    [FunctionName("OrderItemsReserver")]
    public async Task Run([ServiceBusTrigger("orders", Connection = "connectionString")]
        string myQueueItem, ILogger log)
    {
        string connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        string containerName = Environment.GetEnvironmentVariable("ContainerName");

        log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

        var dataInJson = JsonConvert.DeserializeObject<Order>(myQueueItem);

        int i;
        for (i = 0; i < 3; i++)
        {
            try
            {
                var blobClient = new BlobContainerClient(connection, containerName);
                using Stream myBlob = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataInJson, Formatting.Indented)));
                var blob = blobClient.GetBlobClient($"{Guid.NewGuid()}.json");
                await blob.UploadAsync(myBlob);

                break;
            }
            catch (Exception ex)
            {
                log.LogInformation($"error: {ex.Message}");
            }
        }

        if (i == 3)
        {
            var response = await httpClient.PostAsync(logicAppUri, new StringContent(myQueueItem, Encoding.UTF8, "application/json"));
        }
        
    }
}

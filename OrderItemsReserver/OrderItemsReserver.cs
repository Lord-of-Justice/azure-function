using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using Azure.Storage.Blobs;
using System.Text;

namespace OrderItemsReserver
{
    public static class OrderItemsReserver
    {
        [FunctionName("OrderItemsReserver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic dataInJson = JsonConvert.DeserializeObject(requestBody);
            var orderId = dataInJson?.id;
            try
            {
                var blobClient = new BlobContainerClient(connection, containerName);
                using Stream myBlob =  new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataInJson, Formatting.Indented)));
                var blob = blobClient.GetBlobClient($"{Guid.NewGuid()}.json");
                await blob.UploadAsync(myBlob);

            }
            catch (Exception ex)
            {
                log.LogInformation($"error: {ex.Message}");
            }


            log.LogInformation($"OrderItemsReserver HTTP trigger successfully processed for order {orderId}");

            return new OkObjectResult("Func successfully processed");
        }
    }
}

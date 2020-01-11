using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QueueAPI.Models;
using QueueAPI.Services;

namespace QueueAPI
{
    public static class QueueTrigger
    {
        [FunctionName("QueueTrigger")]
        public static async void Run([QueueTrigger("beerrapport", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            try
            {
                MessageModel queueItem = JsonConvert.DeserializeObject<MessageModel>(myQueueItem);

                
                BeerrapportService service = new BeerrapportService();
                await service.CreateRapport(queueItem);

                //QueueService queueService = new QueueService();
                //BeerRapportModel beerRapport = await queueService.GetWeatherAsync(myQueueItem);


                //APIRepository repository = new APIRepository();
                //byte[] testImage = await repository.GetMapURL(location);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}

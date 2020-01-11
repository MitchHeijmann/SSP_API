using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using QueueAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QueueAPI.Services
{
    class QueueService
    {
        public async Task<MessageModel> AddMessage(Coordinate location)
        {
            MessageModel messageModel = new MessageModel { Location = location };
            messageModel.ID = GenerateGUID();
            string parsedLocation = JsonConvert.SerializeObject(messageModel);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference("beerrapport");

            // Create the queue if it doesn't already exist
            await queue.CreateIfNotExistsAsync();

            CloudQueueMessage message = new CloudQueueMessage(parsedLocation);
            await queue.AddMessageAsync(message);
            Console.WriteLine(message.Id);

            return messageModel;
        }

        private string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}

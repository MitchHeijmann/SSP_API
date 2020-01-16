using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QueueAPI.Models;
using QueueAPI.Services;

namespace QueueAPI
{
    public static class Request
    {
        [FunctionName("request")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string longitude, latidude;
            
            try
            {
                longitude = req.Query["lon"];
                latidude = req.Query["lat"];
            }
            catch { return new BadRequestResult(); }

            Coordinate location = new Coordinate { Latidude = latidude, Longitude = longitude };

            QueueService service = new QueueService();
            MessageModel messageAdded = await service.AddMessage(location);

            return messageAdded != null || !string.IsNullOrEmpty(messageAdded.ID)
                ? (ActionResult)new OkObjectResult(messageAdded)
                : new BadRequestObjectResult("Message failed...");
        }
    }
}

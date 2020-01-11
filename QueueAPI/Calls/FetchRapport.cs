using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QueueAPI.Services;

namespace QueueAPI.Calls
{
    public static class FetchRapport
    {
        [FunctionName("FetchRapport")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string blobId;
            try
            {
                blobId = req.Query["blobID"].ToString();
            }
            catch { return new BadRequestResult(); }

            blobId += ".png";
            BeerrapportService service = new BeerrapportService();
            string url = await service.FetchRapportFromBlobAsync(blobId);


            return string.IsNullOrEmpty(url)
                ? (ActionResult)new NoContentResult()
                : new OkObjectResult(url);
        }
    }
}

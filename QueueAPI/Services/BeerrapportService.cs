using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using QueueAPI.Models;

using SixLabors.Fonts;

// pre-release packages!
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace QueueAPI.Services
{
    class BeerrapportService
    {
        private BeerRapportModel BeerRapport;
        public async Task CreateRapport(MessageModel message)
        {
            BeerRapport = new BeerRapportModel();

            WeatherRepository weatherService = new WeatherRepository();
            BeerRapport = await weatherService.GetWeatherAsync(message.Location);
            string imageText = CheckDrinkingTime();

            if (BeerRapport == null)
                return;

            APIRepository repository = new APIRepository();
            BeerRapport.MapURL = await repository.GetMapURL(message.Location);
            BeerRapport.ImageBase64 = DrawImage((imageText, (2, 8)));
            BeerRapport.QueueID = message.ID;

            if (string.IsNullOrEmpty(BeerRapport.MapURL))
                return;

            SaveToBlob();
        }

        private string CheckDrinkingTime()
        {
            if (BeerRapport.TempMin > 14 || BeerRapport.TempMax > 14 || BeerRapport.Temp > 14)
            {
                return $"BIER TIJD! Het is {BeerRapport.Temp} graden. ";
            }
            return $"Helaas, {BeerRapport.Temp} graden is wat te koud voor bier.";
        }

        private string DrawImage(params (string text, (float x, float y) position)[] texts)
        {
            byte[] data = Convert.FromBase64String(BeerRapport.MapURL);
            var memoryStream = new MemoryStream();
            var image = Image.Load(data);

            try
            {
                image
    .Clone(img =>
    {
        foreach (var (text, (x, y)) in texts)
        {

            img.DrawText(text, SystemFonts.CreateFont("Verdana", 18), Rgba32.Red, new PointF(x, y));
        }
    })
    .SaveAsPng(memoryStream);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            memoryStream.Position = 0;
            byte[] bytes = memoryStream.ToArray();
            string base64 = Convert.ToBase64String(bytes);
            return base64;
        }

        private async void SaveToBlob()
        {

            string fileName = $"{BeerRapport.QueueID}.png";

            byte[] photoBytes = Convert.FromBase64String(BeerRapport.ImageBase64);

            CloudStorageAccount storageAccount = CloudStorageAccount
              .Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("beerimages");

            await container.CreateIfNotExistsAsync(
              BlobContainerPublicAccessType.Blob,
              new BlobRequestOptions(),
              new OperationContext());
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            blob.Properties.ContentType = "image/png";

            try
            {
                using (Stream stream = new MemoryStream(photoBytes, 0, photoBytes.Length))
                {
                    await blob.UploadFromStreamAsync(stream).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task<string> FetchRapportFromBlobAsync(string ID)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount
              .Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("beerimages");

            return await GetBlobSasUriAsync(container, ID);
        }

        private static async Task<string> GetBlobSasUriAsync(CloudBlobContainer container, string blobName, string policyName = null)
        {
            string sasBlobToken;

            // Get a reference to a blob within the container.
            // Note that the blob may not exist yet, but a SAS can still be created for it.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            bool excist = await blob.ExistsAsync();
            if (!excist)
                return null;

            if (policyName == null)
            {
                // Create a new access policy and define its constraints.
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
                // to construct a shared access policy that is saved to the container's shared access policies.
                SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                    Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create
                };

                // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                sasBlobToken = blob.GetSharedAccessSignature(adHocSAS);

                Console.WriteLine("SAS for blob (ad hoc): {0}", sasBlobToken);
                Console.WriteLine();
            }
            else
            {
                // Generate the shared access signature on the blob. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);

                Console.WriteLine("SAS for blob (stored access policy): {0}", sasBlobToken);
                Console.WriteLine();
            }

            // Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;
        }
    }
}

using Microsoft.AspNetCore.WebUtilities;
using QueueAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QueueAPI.Services
{
    class APIRepository
    {
        public async Task<string> GetMapURL(Coordinate location)
        {
            // TODO -- Connection to Azure Maps

            string apiUrl = "https://atlas.microsoft.com/map/static/png";
            string apiURLWithParameters = QueryHelpers.AddQueryString(apiUrl, MakeAzureMapAPIURL(location));

            // Create a request for the URL.   
            WebRequest webRequest = WebRequest.Create(apiURLWithParameters);
            webRequest.Method = "GET";

            // If required by the server, set the credentials.  
            webRequest.Credentials = CredentialCache.DefaultCredentials;

            try
            {
                using (var response = await webRequest.GetResponseAsync())
                {
                    using (var reader = new BinaryReader(response.GetResponseStream()))
                    {
                        // Read file 
                        byte[] bytes = reader.ReadBytes(500000);
                        string base64 = Convert.ToBase64String(bytes);
                        return base64;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private Dictionary<string, string> MakeAzureMapAPIURL(Coordinate location)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            string locationFormat = location.Longitude.ToString() + "," + location.Latidude.ToString();
            string centerFormat = "default||" + locationFormat.Replace(',', '+');

            // HARDCODED FOR AZURE MAPS

            parameters.Add("subscription-key", Environment.GetEnvironmentVariable("AzureMapsSubscriptionKey")); // Subscription Key 
            parameters.Add("api-version", "1.0"); // API Version
            parameters.Add("layer", "basic"); // Static Map Layer: basic, hybrid, labels
            parameters.Add("style", "main"); // Image style: basic, dark
            parameters.Add("zoom", "11"); // Zoom level
            parameters.Add("center", locationFormat); // Coordinates of the center point. Format: 'lon,lat'
            parameters.Add("pins", centerFormat); // Pin for user location

            return parameters;
        }
    }
}

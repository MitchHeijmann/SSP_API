using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using SSPBeerRapport.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SSPBeerRapport.Data
{
    public class APIService
    {
        private MessageModel Message;

        public async Task<string> MakeBeerRapport(Coordinate location)
        {
            if (location == null)
                return null;

            try
            {
                Message = await SendRequestAsync(location).ConfigureAwait(true);
                if (Message == null)
                    return null;
            }
            catch { return null; }

            try
            {
                bool founded = false;

                while (!founded)
                {
                    try
                    {
                        string imageUrl = await RetrieveImageAsync(Message.ID);
                        if (imageUrl != null)
                            return imageUrl;
                    }
                    catch { return null; }
                }
            }
            catch { return null; }
            return null;
        }

        private async Task<MessageModel> SendRequestAsync(Coordinate location)
        {
            string apiUrl = "https://beerapissp.azurewebsites.net/api/request";
            string serverResponse = "";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("lon", location.Longitude);
            parameters.Add("lat", location.Latitude);

            string apiURLWithParameters = QueryHelpers.AddQueryString(apiUrl, parameters);

            // Create a request for the URL.   
            WebRequest webRequest = WebRequest.Create(apiURLWithParameters);
            webRequest.Method = "GET";

            //webRequest.Headers.Add("authorization", $"Bearer {accessToken}");

            // If required by the server, set the credentials.  
            webRequest.Credentials = CredentialCache.DefaultCredentials;


            // Get the response.  
            using (WebResponse response = await webRequest.GetResponseAsync())
            {
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.  

                using (StreamReader reader = new StreamReader(dataStream))
                {
                    serverResponse = await reader.ReadToEndAsync();
                }
                response.Close();
            }



            return string.IsNullOrEmpty(serverResponse)
                ? null
                : JsonConvert.DeserializeObject<MessageModel>(serverResponse);

        }

        private async Task<string> RetrieveImageAsync(string id)
        {
            string apiUrl = "https://beerapissp.azurewebsites.net/api/FetchRapport";
            string serverResponse = "";

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "blobID", id }
            };

            string apiURLWithParameters = QueryHelpers.AddQueryString(apiUrl, parameters);

            // Create a request for the URL.   
            WebRequest webRequest = WebRequest.Create(apiURLWithParameters);
            webRequest.Method = "GET";

            //webRequest.Headers.Add("authorization", $"Bearer {accessToken}");

            // If required by the server, set the credentials.  
            webRequest.Credentials = CredentialCache.DefaultCredentials;


            // Get the response.  
            using (HttpWebResponse response = (HttpWebResponse)await webRequest.GetResponseAsync())
            {
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.  

                using (StreamReader reader = new StreamReader(dataStream))
                {
                    serverResponse = await reader.ReadToEndAsync();
                    
                }
                if (response.StatusCode == HttpStatusCode.NoContent)
                    return null;
                
                response.Close();
            }

            

            return string.IsNullOrEmpty(serverResponse)
                ? null
                : serverResponse;
        }
    }
}

using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using QueueAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QueueAPI.Services
{
    class WeatherRepository
    {
        private Coordinate Location;
        public async Task<BeerRapportModel> GetWeatherAsync(Coordinate location)
        {
            Location = location;
            BeerRapportModel beerRapport = new BeerRapportModel();

            string apiUrl = "https://api.openweathermap.org/data/2.5/weather";
            string serverResponse = "";


            string apiURLWithParameters = QueryHelpers.AddQueryString(apiUrl, MakeParamaters());

            // Create a request for the URL.   
            WebRequest webRequest = WebRequest.Create(apiURLWithParameters);
            webRequest.Method = "GET";

            //webRequest.Headers.Add("authorization", $"Bearer {accessToken}");

            // If required by the server, set the credentials.  
            //webRequest.Credentials = CredentialCache.DefaultCredentials;

            var jsonParsed = new JObject();
            // Get the response.  
            using (WebResponse response = await webRequest.GetResponseAsync())
            {
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.  

                using (StreamReader reader = new StreamReader(dataStream))
                {
                    serverResponse = await reader.ReadToEndAsync();
                    jsonParsed = JObject.Parse(serverResponse);
                }
                response.Close();
            }

            beerRapport.Temp = float.Parse(jsonParsed["main"]["temp"].ToString());
            beerRapport.TempMin = float.Parse(jsonParsed["main"]["temp_min"].ToString());
            beerRapport.TempMax = float.Parse(jsonParsed["main"]["temp_max"].ToString());



            return beerRapport;
        }

        private Dictionary<string, string> MakeParamaters()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("lat", Location.Latidude);
            parameters.Add("lon", Location.Longitude);
            parameters.Add("units", "metric");
            parameters.Add("lang", "nl");
            parameters.Add("APPID", Environment.GetEnvironmentVariable("WeatherForecastID"));
            return parameters;
        }
    }
}

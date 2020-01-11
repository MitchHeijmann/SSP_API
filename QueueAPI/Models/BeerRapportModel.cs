using System;
using System.Collections.Generic;
using System.Text;

namespace QueueAPI.Models
{
    class BeerRapportModel
    {
        public float Temp { get; set; }
        public float TempMin { get; set; }
        public float TempMax { get; set; }
        public string Description { get; set; }
        public string WeatherType { get; set; }
        public string MapURL { get; set; }
        public bool BeerTime { get; set; }
        public string QueueID { get; set; }
        public byte[]  ImageBytes { get; set; }
        public string ImageBase64 { get; set; }
    }
}

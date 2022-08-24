using System;
namespace CloudWeather.DataLoader.Models
{
    public class TemperatureModel
    {
        public decimal TempLowF { get; set; }
        public decimal TempHighF { get; set; }
        public string ZipCode { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}


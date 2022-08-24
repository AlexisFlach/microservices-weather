using System;
namespace CloudWeather.Report.Config
{
    public class WeatherDataConfig
    {
        public string PrecipDataProtocol { get; set; } = String.Empty;
        public string PrecipDataHost { get; set; } = String.Empty;
        public string PrecipDataPort { get; set; } = String.Empty;
        public string TempDataProtocol { get; set; } = String.Empty;
        public string TempDataHost { get; set; } = String.Empty;
        public string TempDataPort { get; set; } = String.Empty;
    }
}


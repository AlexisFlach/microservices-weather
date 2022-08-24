using System;
using System.Text.Json;
using CloudWeather.Report.Config;
using CloudWeather.Report.DataAccess;
using CloudWeather.Report.Models;
using Microsoft.Extensions.Options;

namespace CloudWeather.Report.BusinessLogic
{
    /// <summary>
    /// Aggregates data from multiple external sources to build a weather report
    /// </summary>

    public interface IWeatherReportAggregator
    {
        public Task<WeatherReport> BuildWeeklyReport(string zip, int days);
    }

    public class WeatherReportAggregator : IWeatherReportAggregator
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<WeatherReportAggregator> _logger;
        private readonly WeatherDataConfig _weatherDataConfig;
        private readonly WeatherReportDbContext _db;

        public WeatherReportAggregator(
            IHttpClientFactory http,
            ILogger<WeatherReportAggregator> logger,
            IOptions<WeatherDataConfig> weaherConfig,
            WeatherReportDbContext db
            )
        {
            _http = http;
            _logger = logger;
            _weatherDataConfig = weaherConfig.Value;
            _db = db;
        }

        public async Task<WeatherReport> BuildWeeklyReport(string zip, int days)
        {
            var httpClient = _http.CreateClient();
            var precipData = await FetchPrecipData(httpClient, zip, days);
            var totalRain = GetTotalRain(precipData);
            var totalSnow = GetTotalSnow(precipData);

            _logger.LogInformation(
                $"zip = {zip} over last {days} days" +
                $"total snow: {totalSnow}, rain: {totalRain}"
                );

            var tempData = await FetchTempData(httpClient, zip, days);

            var avgHighTemp = tempData.Average(t => t.TempHighF);
            var avgLowTemp = tempData.Average(t => t.TempLowF);

            _logger.LogInformation(
                $"zip = {zip} over last {days} days" +
                $"low temp: {avgLowTemp}, high temp: {avgHighTemp}"
                );

            var weeklyReport = new WeatherReport
            {
                AverageHighF = Math.Round(avgHighTemp, 1),
                AverageLowF = Math.Round(avgLowTemp, 1),
                RainfallTotalInches = totalRain,
                SnowTotalInches = totalSnow,
                ZipCode = zip,
                CreatedOn = DateTime.UtcNow,

            };

            _db.Add(weeklyReport);
            await _db.SaveChangesAsync();

            return weeklyReport;

        }

        private async Task<List<TemperatureModel>> FetchTempData(HttpClient httpClient, string zip, int days)
        {
            var endPoint = BuildTemperatureServiceEndpoint(zip, days);
            var temperatureRecords = await httpClient.GetAsync(endPoint);
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var temperatureData = await temperatureRecords
                .Content
                .ReadFromJsonAsync<List<TemperatureModel>>(jsonSerializerOptions);
            return temperatureData ?? new List<TemperatureModel>();
        }

        private async Task<List<PrecipationModel>> FetchPrecipData(HttpClient httpClient, string zip, int days)
        {
            var endPoint = BuildPrecipitationServiceEndpoint(zip, days);
            var precipRecords = await httpClient.GetAsync(endPoint);
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var precipData = await precipRecords
                .Content
                .ReadFromJsonAsync<List<PrecipationModel>>(jsonSerializerOptions);
            return precipData ?? new List<PrecipationModel>();
        }
        private string BuildTemperatureServiceEndpoint(string zip, int days)
        {
            var tempServiceProtocol = _weatherDataConfig.TempDataProtocol;
            var tempServiceHost = _weatherDataConfig.TempDataHost;
            var tempServicePort = _weatherDataConfig.TempDataPort;
            return $"{tempServiceProtocol}://{tempServiceHost}:{tempServicePort}/observation/{zip}?days={days}";
        }
        private string BuildPrecipitationServiceEndpoint(string zip, int days)
        {
            var precipServiceProtocol = _weatherDataConfig.PrecipDataProtocol;
            var precipServiceHost = _weatherDataConfig.PrecipDataHost;
            var precipServicePort = _weatherDataConfig.PrecipDataPort;
            return $"{precipServiceProtocol}://{precipServiceHost}:{precipServicePort}/observation/{zip}?days={days}";
        }

        private static decimal GetTotalSnow(IEnumerable<PrecipationModel> precipData)
        {
            var totalSnow = precipData
                .Where(p => p.WeatherType == "snow")
                .Sum(p => p.AmountInches);
            return Math.Round(totalSnow, 1);
        }
        private static decimal GetTotalRain(IEnumerable<PrecipationModel> precipData)
        {
            var totalRain = precipData
                .Where(p => p.WeatherType == "rain")
                .Sum(p => p.AmountInches);
            return Math.Round(totalRain, 1);
        }
    }
}



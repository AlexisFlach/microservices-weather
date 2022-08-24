using System.Net.Http.Json;
using CloudWeather.DataLoader.Models;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appSettings.json")
    .AddEnvironmentVariables()
    .Build();

var serviceConfig = config.GetSection("Services");

var tempServiceConfig = serviceConfig.GetSection("Temperature");
var tempServiceHost = tempServiceConfig["Host"];
var tempServicePort = tempServiceConfig["Port"];

var precipServiceConfig = serviceConfig.GetSection("Precipitation");
var preciperviceHost = precipServiceConfig["Host"];
var precipServicePort = precipServiceConfig["Port"];

var zipCodes = new List<string>
{
    "11111",
    "22222"
};

Console.WriteLine("Starting Data Load");

var tempHttpClient = new HttpClient();
tempHttpClient.BaseAddress = new Uri($"http://{tempServiceHost}:{tempServicePort}");

var precipHttpClient = new HttpClient();
precipHttpClient.BaseAddress = new Uri($"http://{preciperviceHost}:{precipServicePort}");

foreach(var zip in zipCodes)
{
    Console.WriteLine($"Processing Zip code: {zip}");
    var from = DateTime.Now.AddDays(-2);
    var thru = DateTime.Now;

    for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
    {
        var temps = PostTemp(zip, day, tempHttpClient);
        PostPrecip(temps[0], zip, day, precipHttpClient);
    }
}

void PostPrecip(int lowTemp, string zip, DateTime day, HttpClient precipHttpClient)
{
    var rand = new Random();
    var isPrecip = rand.Next(2) < 1;
    PrecipitationModel precipitation;
    if(isPrecip)
    {
        var precipInches = rand.Next(1, 16);
        if(lowTemp < 32)
        {
            precipitation = new PrecipitationModel
            {
                AmountInches = precipInches,
                WeatherType = "snow",
                ZipCode = zip,
                CreatedOn = day
            };
        } else
        {
            precipitation = new PrecipitationModel
            {
                AmountInches = precipInches,
                WeatherType = "rain",
                ZipCode = zip,
                CreatedOn = day
            };
        }
    } else
    {
        precipitation = new PrecipitationModel
        {
            AmountInches = 0,
            WeatherType = "none",
            ZipCode = zip,
            CreatedOn = day
        };
    }

    var precipresponse = precipHttpClient
        .PostAsJsonAsync("observation", precipitation)
        .Result;

    if(precipresponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Posted Precipitation: Date: {day:d} " +
            $"Zip: {zip} " +
            $"Type: {precipitation.WeatherType} " +
            $"Amount (in.): {precipitation.AmountInches}"

            ); ;
    }
        
}

List<int> PostTemp(string zip, DateTime day, HttpClient tempHttpClient)
{
    var rand = new Random();
    var t1 = rand.Next(0, 100);
    var t2 = rand.Next(0, 100);
    var hiLoTemps = new List<int> { t1, t2 };
    hiLoTemps.Sort();

    var tempObservation = new TemperatureModel
    {
        TempLowF = hiLoTemps[0],
        TempHighF = hiLoTemps[1],
        ZipCode = zip,
        CreatedOn = day
    };

    var tempResponse = tempHttpClient
        .PostAsJsonAsync("observation", tempObservation)
        .Result;
    if(tempResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Posted Temp: Date: {day:d} " +
            $"Zip: {zip} " +
            $"Low: {hiLoTemps[0]} " +
            $"High: {hiLoTemps[1]} " );
    } else
    {
        Console.WriteLine(tempResponse.ToString());
    }
    return hiLoTemps;
}

































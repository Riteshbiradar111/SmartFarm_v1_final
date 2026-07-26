using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(IHttpClientFactory httpFactory, IMemoryCache cache, ILogger<WeatherService> logger)
        {
            _httpFactory = httpFactory;
            _cache = cache;
            _logger = logger;
        }

        public async Task<WeatherViewModel> GetWeatherAsync(decimal latitude, decimal longitude, bool forceRefresh = false)
        {
            var vm = new WeatherViewModel();
            var key = $"weather_{latitude:F6}_{longitude:F6}";

            if (!forceRefresh && _cache.TryGetValue(key, out WeatherViewModel cached))
            {
                return cached;
            }

            try
            {
                var client = _httpFactory.CreateClient();
                // Request current weather and 7-day daily forecast
                var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current_weather=true&daily=temperature_2m_max,temperature_2m_min,precipitation_probability_max&timezone=auto&forecast_days=7";
                var resp = await client.GetAsync(url);
                resp.EnsureSuccessStatusCode();
                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                var root = doc.RootElement;

                // Current
                if (root.TryGetProperty("current_weather", out var current))
                {
                    vm.Current = new WeatherCurrent();
                    if (current.TryGetProperty("temperature", out var t)) vm.Current.Temperature = t.GetDecimal();
                    if (current.TryGetProperty("windspeed", out var w)) vm.Current.WindSpeed = w.GetDecimal();
                    if (current.TryGetProperty("time", out var time)) vm.Current.Time = DateTime.Parse(time.GetString() ?? "");
                    if (current.TryGetProperty("weathercode", out var wc)) vm.Current.Condition = MapWeatherCodeToText(wc.GetInt32());
                }

                // Some APIs expose relative humidity and rain only in other sections; try 'hourly' -> 'relativehumidity_2m' or others if needed
                if (root.TryGetProperty("hourly", out var hourly))
                {
                    if (hourly.TryGetProperty("relativehumidity_2m", out var rhArr) && rhArr.ValueKind == JsonValueKind.Array)
                    {
                        var rhElem = rhArr.EnumerateArray().FirstOrDefault();
                        if (rhElem.ValueKind != JsonValueKind.Undefined) vm.Current.Humidity = rhElem.GetDecimal();
                    }
                }

                // Sunrise/Sunset may be available in 'daily' or separate fields; try daily 'sunrise'/'sunset'
                if (root.TryGetProperty("daily", out var daily))
                {
                    var times = daily;
                    if (times.TryGetProperty("time", out var timeArr) && timeArr.ValueKind == JsonValueKind.Array)
                    {
                        var dates = timeArr.EnumerateArray().Select(e => e.GetString()).ToList();
                        var tmins = times.GetProperty("temperature_2m_min").EnumerateArray().Select(x => x.GetDecimal()).ToList();
                        var tmaxs = times.GetProperty("temperature_2m_max").EnumerateArray().Select(x => x.GetDecimal()).ToList();
                        var precs = times.GetProperty("precipitation_probability_max").EnumerateArray().Select(x => x.GetDecimal()).ToList();

                        vm.Forecast7 = new List<WeatherForecastDay>();
                        for (int i = 0; i < dates.Count; i++)
                        {
                            if (DateTime.TryParse(dates[i], out var dt))
                            {
                                vm.Forecast7.Add(new WeatherForecastDay
                                {
                                    Date = dt.Date,
                                    TempMin = tmins.ElementAtOrDefault(i),
                                    TempMax = tmaxs.ElementAtOrDefault(i),
                                    RainProbability = precs.ElementAtOrDefault(i)
                                });
                            }
                        }
                    }
                }

                // Try sunrise/sunset from 'daily' if available
                if (root.TryGetProperty("daily", out var daily2))
                {
                    if (daily2.TryGetProperty("sunrise", out var sunriseArr) && sunriseArr.ValueKind == JsonValueKind.Array)
                    {
                        var s = sunriseArr.EnumerateArray().FirstOrDefault().GetString();
                        if (DateTime.TryParse(s, out var sd)) vm.Current.Sunrise = sd;
                    }
                    if (daily2.TryGetProperty("sunset", out var sunsetArr) && sunsetArr.ValueKind == JsonValueKind.Array)
                    {
                        var s = sunsetArr.EnumerateArray().FirstOrDefault().GetString();
                        if (DateTime.TryParse(s, out var sd)) vm.Current.Sunset = sd;
                    }
                }

                // Try precipitation from 'current' or 'hourly'
                if (root.TryGetProperty("current_weather", out var cw) && cw.TryGetProperty("temperature", out _))
                {
                    // nothing additional here
                }

                // Cache result for 20 minutes
                _cache.Set(key, vm, TimeSpan.FromMinutes(20));
                return vm;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Weather fetch failed for coords {lat},{lon}", latitude, longitude);
                return new WeatherViewModel { ErrorMessage = "Weather data unavailable" };
            }
        }

        private string MapWeatherCodeToText(int code)
        {
            // Simplified mapping for common codes
            return code switch
            {
                0 => "Clear",
                1 or 2 or 3 => "Partly Cloudy",
                45 or 48 => "Fog",
                51 or 53 or 55 => "Drizzle",
                61 or 63 or 65 => "Rain",
                71 or 73 or 75 => "Snow",
                95 => "Thunderstorm",
                _ => "Cloudy",
            };
        }
    }
}

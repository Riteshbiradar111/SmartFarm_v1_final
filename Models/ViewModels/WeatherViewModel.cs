using System;
using System.Collections.Generic;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels
{
    public class WeatherCurrent
    {
        public decimal? Temperature { get; set; }
        public decimal? Humidity { get; set; }
        public decimal? Rain { get; set; }
        public decimal? WindSpeed { get; set; }
        public string? Condition { get; set; }
        public DateTime? Sunrise { get; set; }
        public DateTime? Sunset { get; set; }
        public DateTime? Time { get; set; }
    }

    public class WeatherForecastDay
    {
        public DateTime Date { get; set; }
        public decimal? TempMin { get; set; }
        public decimal? TempMax { get; set; }
        public decimal? RainProbability { get; set; }
        public int? WeatherCode { get; set; }
    }

    public class WeatherViewModel
    {
        public WeatherCurrent? Current { get; set; }
        public List<WeatherForecastDay> Forecast7 { get; set; } = new List<WeatherForecastDay>();
        public string? ErrorMessage { get; set; }
    }
}

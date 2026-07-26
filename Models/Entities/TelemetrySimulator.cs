using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;

namespace SmartFarmMVC.Models
{
    public class TelemetrySimulator
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Fetches real-time weather from Open-Meteo for the plot coordinates and simulates 
        /// soil parameters (SoilMoisture, SoilPH, Nitrogen, Phosphorus, Potassium, 
        /// ElectricalConductivity, OrganicCarbon) based on temperature and rain values.
        /// Saves or updates the reading in the SensorReading table.
        /// </summary>
        public static async Task<SensorReading> SimulateReadingAsync(int plotId, decimal latitude, decimal longitude, SmartFarmDbContext context)
        {
            double temperature = 25.0; // Default fallback
            double rain = 0.0;         // Default fallback
            double rainProbability = 15.0; // Default fallback

            try
            {
                // Open-Meteo requires no API key, but a descriptive User-Agent is good practice
                if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                {
                    _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SmartFarmTelemetrySimulator/1.0");
                }

                // Format coordinates to 4 decimal places for the API call
                string latStr = latitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
                string lonStr = longitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);

                string url = $"https://api.open-meteo.com/v1/forecast?latitude={latStr}&longitude={lonStr}&current=temperature_2m,rain&hourly=rain_probability";

                string responseString = await _httpClient.GetStringAsync(url);

                using (JsonDocument doc = JsonDocument.Parse(responseString))
                {
                    var root = doc.RootElement;

                    // Parse current conditions
                    if (root.TryGetProperty("current", out var currentEl))
                    {
                        if (currentEl.TryGetProperty("temperature_2m", out var tempProp))
                        {
                            temperature = tempProp.GetDouble();
                        }
                        if (currentEl.TryGetProperty("rain", out var rainProp))
                        {
                            rain = rainProp.GetDouble();
                        }
                    }

                    // Parse hourly rain probability
                    if (root.TryGetProperty("hourly", out var hourlyEl) &&
                        hourlyEl.TryGetProperty("rain_probability", out var rainProbEl))
                    {
                        int count = 0;
                        double sum = 0;
                        foreach (var item in rainProbEl.EnumerateArray())
                        {
                            if (count >= 12) break; // Look at next 12 hours
                            sum += item.GetDouble();
                            count++;
                        }
                        if (count > 0)
                        {
                            rainProbability = sum / count;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback gracefully without throwing, using baseline simulation
                Console.WriteLine($"[TelemetrySimulator] API request failed, using baseline simulation. Error: {ex.Message}");
            }

            var random = new Random();

            // 1. Soil Moisture: Rain increases moisture, high temperatures dry it out
            double baseMoisture = 45.0 + (rain * 10.0) + (rainProbability * 0.25) - (Math.Max(0.0, temperature - 25.0) * 0.5);
            decimal soilMoisture = (decimal)Math.Clamp(baseMoisture + (random.NextDouble() * 12.0 - 6.0), 10.0, 95.0);

            // 2. Soil pH: Dilution by rain makes it slightly more neutral/acidic, baseline is around 6.7
            double basePH = 6.7 + (rain * 0.05) - (rainProbability * 0.005);
            decimal soilPH = (decimal)Math.Clamp(basePH + (random.NextDouble() * 1.0 - 0.5), 5.0, 8.5);

            // 3. Nitrogen (N): Dynamic, temperature enhances microbial activity slightly increasing availability
            double tempFactor = Math.Clamp((temperature - 10.0) / 25.0, 0.0, 1.2);
            decimal nitrogen = (decimal)Math.Clamp(35.0 + (tempFactor * 40.0) + (random.NextDouble() * 50.0), 15.0, 140.0);

            // 4. Phosphorus (P): Moderately stable, slightly affected by soil moisture
            double moistureFactor = (double)soilMoisture / 100.0;
            decimal phosphorus = (decimal)Math.Clamp(20.0 + (moistureFactor * 20.0) + (random.NextDouble() * 30.0), 5.0, 90.0);

            // 5. Potassium (K): Mobile nutrient, increases slightly with optimal temperature
            decimal potassium = (decimal)Math.Clamp(100.0 + (tempFactor * 60.0) + (random.NextDouble() * 100.0), 40.0, 320.0);

            // 6. Electrical Conductivity (EC): Rain washes away/dilutes salts (lower EC), evaporation concentrates (higher EC)
            double baseEC = 1.6 + (Math.Max(0.0, temperature - 20.0) * 0.03) - (rain * 0.15);
            decimal electricalConductivity = (decimal)Math.Clamp(baseEC + (random.NextDouble() * 0.5 - 0.25), 0.2, 3.5);

            // 7. Organic Carbon (OC): Very stable, simulated with minor local variance
            decimal organicCarbon = (decimal)Math.Clamp(0.85 + (random.NextDouble() * 0.6 - 0.3), 0.2, 2.2);

            // Save to SensorReading table
            var reading = context.SensorReadings.FirstOrDefault(r => r.PlotId == plotId);
            bool isNew = false;
            if (reading == null)
            {
                reading = new SensorReading { PlotId = plotId };
                isNew = true;
            }

            reading.SoilMoisture = Math.Round(soilMoisture, 2);
            reading.SoilPH = Math.Round(soilPH, 2);
            reading.Nitrogen = Math.Round(nitrogen, 2);
            reading.Phosphorus = Math.Round(phosphorus, 2);
            reading.Potassium = Math.Round(potassium, 2);
            reading.ElectricalConductivity = Math.Round(electricalConductivity, 2);
            reading.OrganicCarbon = Math.Round(organicCarbon, 2);
            reading.LastUpdated = DateTime.Now;

            if (isNew)
            {
                context.SensorReadings.Add(reading);
            }

            await context.SaveChangesAsync();
            return reading;
        }
    }
}

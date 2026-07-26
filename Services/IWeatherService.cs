using System.Threading.Tasks;
using Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Services
{
    public interface IWeatherService
    {
        Task<WeatherViewModel> GetWeatherAsync(decimal latitude, decimal longitude, bool forceRefresh = false);
    }
}

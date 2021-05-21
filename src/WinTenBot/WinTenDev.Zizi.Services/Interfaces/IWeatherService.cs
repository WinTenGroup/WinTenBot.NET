using System.Threading.Tasks;
using WinTenDev.Zizi.Models.Types;

namespace WinTenDev.Zizi.Services.Interfaces
{
    public interface IWeatherService
    {
        Task<CurrentWeather> GetWeatherAsync(float lat, float lon);
    }
}
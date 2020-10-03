using System.Threading.Tasks;
using Zizi.Bot.Models;

namespace Zizi.Bot.Interfaces
{
    interface IWeatherService
    {
        Task<CurrentWeather> GetWeatherAsync(float lat, float lon);
    }
}
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Interfaces;
using Zizi.Bot.Services.Features;

namespace Zizi.Bot.Handlers
{
    class WeatherReporter : IUpdateHandler
    {
        private readonly IWeatherService _weatherService;
        private readonly TelegramService _telegramService;

        public WeatherReporter(
            IWeatherService weatherService,
            TelegramService telegramService
        )
        {
            _telegramService = telegramService;
            _weatherService = weatherService;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var location = _telegramService.Message.Location;

            var weather = await _weatherService.GetWeatherAsync(location.Latitude, location.Longitude);

            var text = $"Weather status is <b>{weather.Status}</b> with the temperature of {weather.Temp:F1}.\n" +
                       $"Min: {weather.MinTemp:F1}\n" +
                       $"Max: {weather.MaxTemp:F1}\n\n" +
                       "powered by <a href='https://www.metaweather.com'>MetaWeather</a>";

            await _telegramService.SendTextAsync(text);
        }
    }
}
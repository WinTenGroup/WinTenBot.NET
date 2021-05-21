using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using WinTenDev.WebHook.Host.Models.Github;
using WinTenDev.WebHook.Host.Services;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.WebHook.Host.Controllers
{
    [ApiController]
    [Route("/")]
    public class HookController : ControllerBase
    {
        private readonly ITelegramService _telegramService;

        public HookController(ITelegramService telegramServiceService)
        {
            _telegramService = telegramServiceService;
        }


        // GET
        [HttpGet]
        public IActionResult Index()
        {
            // return View();
            return Ok("Jangkrik");
        }

        [HttpPost("/debug")]
        public IActionResult Debug([FromBody] object content, [FromQuery] object query)
        {
            var stopwatch = Stopwatch.StartNew();

            var jsonName = HttpContext.TraceIdentifier + ".json";
            content.WriteToFileAsync(jsonName);

            Log.Debug("Content: {Content}", content);
            Log.Debug("Query: {Query}", query);

            if (content == null)
            {
                Log.Information("Content is null!");
                return Ok();
            }

            var json = content.ToString() ?? string.Empty;

            Log.Information("Receiving hook");

            var msgSb = new StringBuilder();

            var rootZen = JsonConvert.DeserializeObject<GithubZen>(json);
            var rootAction = JsonConvert.DeserializeObject<RootAction>(json);

            if (rootAction.Action.IsNotNullOrEmpty())
            {
                var actionStr = rootAction.Action;

                switch (actionStr)
                {
                    case "created":
                    case "started":
                    case "deleted":
                        msgSb.AppendLine($"Someone {actionStr} repo");
                        break;
                }
            }
            else if (rootZen.Zen.IsNotNullOrEmpty())
            {
                var zen = rootZen.Zen;

                msgSb.AppendLine(rootZen.Zen);
            }
            else
            {
                msgSb.AppendLine("Undetected Hook");
            }

            var msgText = msgSb.ToTrimmedString();

            _telegramService.SendMessage(-1001404591750, msgText);

            stopwatch.Stop();

            return new JsonResult(new
            {
                StatusCode = 200,
                Elapsed = stopwatch.Elapsed
            });
        }

        [HttpGet("/about")]
        public OkObjectResult About()
        {
            return Ok("About");
        }
    }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using WinTenDev.WebHook.Models.Github;
using WinTenDev.WebHook.Services;

namespace WinTenDev.WebHook.Controllers
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

            Log.Debug("Content: {Content}", content);
            Log.Debug("Query: {Query}", query);

            Log.Information("Receiving hook");
            var github = GithubAdmin.FromJson(content.ToString());
            var zen = github.Zen;
            _telegramService.SendMessage(-1001404591750, zen);

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
#region

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Zizi.Hook.Models.Github;
using Zizi.Hook.Services;

#endregion

namespace Zizi.Hook.Controllers
{
    [ApiController]
    [Route("/")]
    public class HookController : ControllerBase
    {
        public HookController(ITelegramService telegramService)
        {
            Telegram = telegramService;
        }

        private ITelegramService Telegram
        {
            get;
        }

        // GET
        [HttpGet]
        public IActionResult Index()
        {
            // return View();
            return Ok("Jangkrik");
        }

        [HttpPost("/debug")]
        public IActionResult Debug([FromBody] object content)
        {
            var stopwatch = Stopwatch.StartNew();

            Log.Information("Receiving hook");
            var github = GithubAdmin.FromJson(content.ToString());
            var zen = github.Zen;
            Telegram.SendMessage(-1001404591750, zen);

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
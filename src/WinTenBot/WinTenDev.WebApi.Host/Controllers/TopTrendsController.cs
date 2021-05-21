using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Canducci.SqlKata.Dapper.Extensions.SoftBuilder;
using Canducci.SqlKata.Dapper.MySql;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using WinTenDev.WebApi.Host.Models;

namespace WinTenDev.WebApi.Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopTrendsController : Controller
    {
        private readonly IDbConnection _dbConnection;

        public TopTrendsController(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        [HttpGet("top-30d")]
        public async Task<JsonResult> TopActivityLast30Days()
        {
            var stopwatch = Stopwatch.StartNew();

            var model = await _dbConnection
                .SoftBuild()
                .From("zizibot_data.view_top_hit_activity_last_30d")
                .ListAsync<TopTrendActivity>();
            var topTrendActivities = model.ToList();
            
            stopwatch.Stop();
            Log.Information("Elapsed. {Elapsed}", stopwatch.Elapsed);

            return Json(new
            {
                Data = topTrendActivities,
                Count = topTrendActivities.Count()
            });
        }
    }
}
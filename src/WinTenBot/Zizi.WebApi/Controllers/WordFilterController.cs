using System.Data;
using System.Threading.Tasks;
using Canducci.SqlKata.Dapper.Extensions.SoftBuilder;
using Canducci.SqlKata.Dapper.MySql;
using Microsoft.AspNetCore.Mvc;
using WinTenBot.Model;

namespace Zizi.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WordFilterController : Controller
    {
        private readonly IDbConnection _dbConnection;

        public WordFilterController(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        
        // GET
        public async Task<IActionResult> Index()
        {
            var model = await _dbConnection
                .SoftBuild()
                .From("word_filter")
                .ListAsync<WordFilter>();
            
            return Json(model);
        }
    }
}
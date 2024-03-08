using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using System.Diagnostics;
using yazlab2proje1.Models;

namespace yazlab2proje1.Controllers
{
    public class HomeController : Controller
    {
        DatabaseApp dbApp=new DatabaseApp();
        List<Article> articles= new List<Article>();
        
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            dbApp.updateElasticSearch();
            await dbApp.getArticlesAsync();

            return View(dbApp.articleList);
        }
        
       


        

        public IActionResult Article(ObjectId id)
        {

            return View(dbApp.GetArticleById(id));
        }
        public async Task<IActionResult> SearchResult(string search)
        {
            return View(dbApp.searchEngine(search));
           
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
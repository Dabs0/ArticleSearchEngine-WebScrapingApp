using Business.Abstract;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using Nest;
using System.Diagnostics;
using yazlab2proje1.Models;

namespace yazlab2proje1.Controllers
{
    public class HomeController : Controller
    {
      //  DatabaseApp dbApp = new DatabaseApp();

        private readonly IAkademikYayinService _akademikYayinService;
        public HomeController(IAkademikYayinService akademikYayinService, ILogger<HomeController> logger)
        {
            _akademikYayinService = akademikYayinService;
            _logger = logger;
        }


        private readonly ILogger<HomeController> _logger;

  
        //Ana sayfa
        public async Task<IActionResult> Index()
        {

            _akademikYayinService.updateElasticSearch();
            await _akademikYayinService.getArticlesAsync();

            return View(_akademikYayinService.getArticleList());
        }
        //Yayın sayfası
        public IActionResult Article(ObjectId id)
        {

            return View(_akademikYayinService.GetArticleById(id));
        }
        //Arama sonuç sayfası
        public async Task<IActionResult> SearchResult(string search, string? yearMin=null, string? yearMax = null, bool research = true, bool review = true, bool conference = true, bool book = true)
        {
            List<Article> results= _akademikYayinService.searchEngine(search);
            // Filtreleme işlemleri
            if (!string.IsNullOrEmpty(yearMin))
            {
                int minYear = int.Parse(yearMin);
                results = results.Where(article => article.publishDate.Year >= minYear).ToList();
            }
            if (!string.IsNullOrEmpty(yearMax))
            {
                int maxYear = int.Parse(yearMax);
                results = results.Where(article => article.publishDate.Year <= maxYear).ToList();
            }
            if (research)
            {
                results = results.Where(article => !article.type.Equals("Makale")).ToList();
            }
            if (review)
            {
                results = results.Where(article => !article.type.Equals("Derleme")).ToList();
            }
            if (conference)
            {
                results = results.Where(article => !article.type.Equals("Konferans Bildirisi")).ToList();
            }
            if (book)
            {
                results = results.Where(article => !article.type.Equals("Kitap")).ToList();
            }

            return View(results);


            /*foreach(Article result in results)
            {
                if(yearMin!=null && result.publishDate.Year<Convert.ToInt32(yearMin) )
                {
                    filteredResults.Remove(result);
                }
                if(yearMax!=null && result.publishDate.Year > Convert.ToInt32(yearMax))
                {
                    filteredResults.Remove(result);
                }
                switch (result.type)
                {
                    case "Makale":
                        if (!research)
                            filteredResults.Remove(result);
                        break;
                    case "Konferans Bildirisi":
                        if (!conference)
                            filteredResults.Remove(result);
                        break;
                    case "Kitap":
                        if (!book)
                            filteredResults.Remove(result);
                        break;
                    case "Derleme":
                        if (!review)
                            filteredResults.Remove(result);
                        break;
                    default: 
                        break;
                }
            }*/

            
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
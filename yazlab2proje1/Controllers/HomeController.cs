using Business.Abstract;
using Entities.Concrete;
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
            await _akademikYayinService.getDBArticlesAsync();

            return View(_akademikYayinService.getArticleList());
        }
        //Yayın sayfası
        public IActionResult Article(ObjectId id)
        {

            return View(_akademikYayinService.GetArticleById(id));
        }
		
		//Arama sonuç sayfası
		public async Task<IActionResult> SearchResult(string search, string? yearMin = null, string? yearMax = null, List<string> selectedTypes = null)
		{
            await _akademikYayinService.getDBArticlesAsync();
            
            
			List<AkademikYayin> results = await _akademikYayinService.searchEngineAsync(search);

            foreach(AkademikYayin yayin in _akademikYayinService.getArticleList())
            {
                if(yayin.yayinTurus!=null)
                _akademikYayinService.checkIfYayinTuruExists(yayin.yayinTurus);
            }
			await _akademikYayinService.getYayinTurleriAsync();


			ViewBag.SearchValue = search;
			ViewBag.MinYearValue = yearMin;
			ViewBag.MaxYearValue = yearMax;
			ViewBag.YayinTurleri = _akademikYayinService.getYayinTurleriList();



			if (!string.IsNullOrEmpty(yearMin))
			{
				int minYear = int.Parse(yearMin);
				results = results.Where(article => article.yayinYili >= minYear).ToList();
			}
			if (!string.IsNullOrEmpty(yearMax))
			{
				int maxYear = int.Parse(yearMax);
				results = results.Where(article => article.yayinYili <= maxYear).ToList();
			}
			// Eğer hiçbir tür işaretlenmemişse veya hiç tür işaretlenmemişse
			if (selectedTypes == null || selectedTypes.Count == 0)
			{
				return View(results);
			}

			// Seçilen yayın türlerine göre filtreleme yap
			results = results.Where(article => selectedTypes.Contains(article.yayinTurus.YayinTuruAd)).ToList();

			
			
			
			
			
			return View(results);




        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
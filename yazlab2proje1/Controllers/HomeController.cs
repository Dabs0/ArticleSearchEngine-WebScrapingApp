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

        List<Article> articles= new List<Article>();
        List<Article> articleList = new List<Article>();
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            await getArticlesAsync();

            return View(articleList);
        }
        public async Task getArticlesAsync()
    {
            const string connectionUri = "mongodb://localhost:27017/";
            var client = new MongoClient(connectionUri);

            // Veritabanını seç
            var database = client.GetDatabase("local");

            // Koleksiyonu seç
            var collection = database.GetCollection<BsonDocument>("webscraping");

            // Tüm makaleleri getir
            var filter = new BsonDocument();
            var cursor = await collection.FindAsync(filter);
            var articles = await cursor.ToListAsync();

            // Makaleleri Article modeli şeklinde bir listede tut
            
            foreach (var article in articles)
            {
                Article modelArticle = new Article
                {
                    objectId = article.GetValue("_id").AsObjectId, // "_id" alanı integer olarak kabul edilmiş olsun
                    Id = article.GetValue("Id").AsInt32, // "_id" alanı integer olarak kabul edilmiş olsun
                    title= article.GetValue("title").AsString,
                    authors = article.GetValue("authors").AsBsonArray.ToJson().Trim('[', ']').Split(", "),
                    type = article.GetValue("type").AsString,
                    publishDate = article.GetValue("publishDate").AsString,
                    publisher = article.GetValue("publisher").AsString,
                    keywords = article.GetValue("keywords").AsBsonArray.ToJson().Trim('[', ']').Split(", "),
                    summary = article.GetValue("summary").AsString,
                    refs = article.GetValue("refs").AsBsonArray.ToJson().Trim('[', ']').Split(", "),
                    citNumber = article.GetValue("citNumber").AsInt32,
                    doi = article.GetValue("doi").AsString,
                    url = article.GetValue("url").AsString
                };
                articleList.Add(modelArticle);
            }

            // Listeyi yazdır
            Console.WriteLine("Makale Listesi:");
            foreach (var article in articleList)
            {
                Console.WriteLine($"{article.Id}, {article.title}, {string.Join(", ", article.authors)}");
            }
            
        }


        private Article GetArticleById(int id)
        {
            const string connectionUri = "mongodb://localhost:27017/";
            var client = new MongoClient(connectionUri);

            // Veritabanını seç
            var database = client.GetDatabase("local");

            // Koleksiyonu seç
            var collection = database.GetCollection<BsonDocument>("webscraping");

            // Belirli bir ID'ye göre makaleyi filtrele
            var filter = Builders<BsonDocument>.Filter.Eq("Id", id); // Varsayılan olarak MongoDB'de ID "_id" alanı olarak saklanır

            // Makaleyi bul
            var articleDocument = collection.Find(filter).FirstOrDefault();

            if (articleDocument != null)
            {
                // Article modeline dönüştür
                Article article = new Article
                {
                    objectId = articleDocument.GetValue("_id").AsObjectId, // "_id" alanı integer olarak kabul edilmiş olsun
                    Id = articleDocument.GetValue("Id").AsInt32, // "_id" alanı integer olarak kabul edilmiş olsun
                    title = articleDocument.GetValue("title").AsString,
                    authors = articleDocument.GetValue("authors").AsBsonArray.Select(a => a.ToString()).ToArray(), // Yazarlar dizisi string'e dönüştürülür
                    type = articleDocument.GetValue("type").AsString,
                    publishDate = articleDocument.GetValue("publishDate").AsString,
                    publisher = articleDocument.GetValue("publisher").AsString,
                    keywords = articleDocument.GetValue("keywords").AsBsonArray.Select(k => k.ToString()).ToArray(), // Keywords dizisi string'e dönüştürülür
                    summary = articleDocument.GetValue("summary").AsString,
                    refs = articleDocument.GetValue("refs").AsBsonArray.Select(r => r.ToString()).ToArray(), // Refs dizisi string'e dönüştürülür
                    citNumber = articleDocument.GetValue("citNumber").AsInt32,
                    doi = articleDocument.GetValue("doi").AsString,
                    url = articleDocument.GetValue("url").AsString
                };

                return article;
            }

            return null; // Makale bulunamazsa null döndür
        }

        public IActionResult Article(int id)
        {

            return View(GetArticleById(id));
        }
        public async Task<IActionResult> SearchResult()
        {


            await getArticlesAsync();
            return View(articleList);
           
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
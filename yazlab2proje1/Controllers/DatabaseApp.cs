//using MongoDB.Bson;
//using MongoDB.Driver;
//using Nest;
//using System.Globalization;
//using System.Net;
//using System.Text;
//using System.Web;
//using System.Xml;
//using yazlab2proje1.Models;
//using HtmlAgilityPack;
//using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
//using System.Net.Http;
//using System.Security.Cryptography.Xml;
//using Entities.Concrete;
//using DataAccess.Abstarct;
//using Business.Abstract;
//using Core.Models;
//using Business.Concrete;

//namespace yazlab2proje1.Controllers
//{
//    public class DatabaseApp : IAkademikYayinService
//    {
//        private readonly IAkademikYayinDataAccess _akademikYayinDataAccess= new IAkademikYayinDataAccess();
//        private readonly IAkademikYayinService _akademikYayinService;
        
//        AkademikYayinManager manager = new AkademikYayinManager(_akademikYayinDataAccess,);
        
//        public DatabaseApp(IAkademikYayinService akademikYayinService)
//        {
//            _akademikYayinService = akademikYayinService;
//        }
//        public DatabaseApp()
//        {

//        }
        
//        public List<Article> articleList = new List<Article>();
//        List<string> keywordList = new List<string>();
//        List<string> referenceList = new List<string>();
//        List<string> yayincilar = new List<string>();
//        List<string> yazarlar = new List<string>();
//        List<string> aramaAnahtarKelime = new List<string>();

//        //veritabanından koleksiyonu al
//        public IMongoCollection<BsonDocument> getCollection()
//        {
//            const string connectionUri = "mongodb://localhost:27017/";
//            var client = new MongoClient(connectionUri);


//            var database = client.GetDatabase("local");


//            var collection = database.GetCollection<BsonDocument>("webscraping");
//            return collection;
//        }

//        //elastic client'i al
//        public ElasticClient getElasticClient()
//        {
//            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("webscraping_index");
//            var elasticClient = new ElasticClient(settings);
//            return elasticClient;
//        }


//        //bson dosyasını article'a çevir
//        public Article createModelArticle(BsonDocument article)
//        {

//            return new Article
//            {
//                objectId = article.GetValue("_id").AsObjectId,
//                Id = article.GetValue("Id").AsInt32,
//                title = article.GetValue("title").AsString,
//                authors = article.GetValue("authors").AsBsonArray.ToJson().Trim('[', ']').Split(", "),
//                type = article.GetValue("type").AsString,
//                publishDate = DateTime.ParseExact(article.GetValue("publishDate").AsString, "yyyy-MM-dd", CultureInfo.InvariantCulture),
//                publisher = article.GetValue("publisher").AsString,
//                keywords = article.GetValue("keywords").AsBsonArray.ToJson().Trim('[', ']').Split(", "),
//                summary = article.GetValue("summary").AsString,
//                refs = article.GetValue("refs").AsBsonArray.ToJson().Trim('[', ']').Split(", "),
//                citNumber = article.GetValue("citNumber").AsInt32,
//                doi = article.GetValue("doi").AsString,
//                url = article.GetValue("url").AsString
//            };
//        }

//        //Yayınları veritabanından al
//        public async Task getArticlesAsync()
//        {


//            // Tüm makaleleri getir
//            var filter = new BsonDocument();
//            var cursor = await getCollection().FindAsync(filter);
//            var articles = await cursor.ToListAsync();

//            // Makaleleri Article modeli şeklinde bir listede tut
//            articleList.Clear();
//            foreach (var article in articles)
//            {

//                articleList.Add(createModelArticle(article));
//            }

//            // Listeyi yazdır
//            Console.WriteLine("Makale Listesi:");
//            foreach (var article in articleList)
//            {
//                Console.WriteLine($"{article.Id}, {article.title}, {string.Join(", ", article.authors)}");
//            }

//        }

//        //Yayınları id'sine göre al
//        public Article GetArticleById(ObjectId id)
//        {


//            // Belirli bir ID'ye göre makaleyi filtrele
//            var filter = Builders<BsonDocument>.Filter.Eq("_id", id); // Varsayılan olarak MongoDB'de ID "_id" alanı olarak saklanır

//            // Makaleyi bul
//            var articleDocument = getCollection().Find(filter).FirstOrDefault();

//            if (articleDocument != null)
//            {
//                // Article modeline dönüştür


//                return createModelArticle(articleDocument);
//            }

//            return null; // Makale bulunamazsa null döndür
//        }

//        //elastic search verisini güncelle
//        public void updateElasticSearch()
//        {
//            // MongoDB'den belgeleri al
//            var mongoDocuments = getCollection().Find(Builders<BsonDocument>.Filter.Empty).ToList();

//            // MongoDB belgelerini Elasticsearch belgelerine dönüştür ve Elasticsearch'e ekle
//            var elasticDocuments = new List<Article>();
//            foreach (var articleDocument in mongoDocuments)
//            {
//                elasticDocuments.Add(createModelArticle(articleDocument));
//            }


//            foreach (var elasticDocument in elasticDocuments)
//            {
//                var indexResponse = getElasticClient().IndexDocument(elasticDocument);
//                if (!indexResponse.IsValid)
//                {
//                    Console.WriteLine($"Hata: {indexResponse.DebugInformation}");
//                }
//            }

//            Console.WriteLine("Belgeler Elasticsearch'e başarıyla eklendi.");
//        }

//        //kelimeyi elasticsearch le ara
//        public List<Article> searchEngine(string searchString)
//        {
//            //webScrapingAsync(searchString).Wait();

//            _akademikYayinService.EklemeYap(searchString);

//            var searchResponse = getElasticClient().Search<Article>(s => s
//                .Query(q => q
//                        .MultiMatch(mm => mm
//                        .Query(searchString)
//                        .Fields(f => f
//                            .Field(ff => ff.title)
//                            .Field(ff => ff.authors)
//                            .Field(ff => ff.type)
//                            .Field(ff => ff.publisher)
//                            .Field(ff => ff.keywords)
//                            .Field(ff => ff.summary)
//                            .Field(ff => ff.refs)
//                            .Field(ff => ff.doi)
//                            .Field(ff => ff.url)
//                        )
//                    )
//                )
//            );

//            if (searchResponse.IsValid)
//            {
//                List<Article> foundArticles = new List<Article>();
//                foreach (var hit in searchResponse.Hits)
//                {

//                    Console.WriteLine($"Belge Id: {hit.Id}, Başlık: {hit.Source.title}");

//                    foundArticles.Add(hit.Source);
//                }
//                return foundArticles;
//            }
//            else
//            {
//                Console.WriteLine($"Arama sırasında bir hata oluştu: {searchResponse.DebugInformation}");
//            }
//            return null;
//        }

//        public async Task webScrapingAsync(string keyword)
//        {


//            // Google Scholar'da arama yapmak için URL oluştur
//            string url = $"https://dergipark.org.tr/tr/search?q={HttpUtility.UrlEncode(keyword)}";

//            var httpClient = new HttpClient(); //get işlemi için oluşturulur
//            var response = await httpClient.GetAsync(url); // içerdeki url ye erişmek için kullanılır
//            var content = await response.Content.ReadAsStringAsync(); // yukarda eriştiğimiz stringi kullanmak için oluşturulur.

//            var htmlDocument = new HtmlDocument();
//            htmlDocument.LoadHtml(content);

//            var resultLinks = htmlDocument.DocumentNode
//         .SelectNodes("//h5[@class='card-title']/a/@href")
//         ?.Select(link => link.GetAttributeValue("href", string.Empty))
//         .ToList();





//            if (resultLinks != null)
//            {
//                Console.WriteLine($"Toplam {resultLinks.Count} arama sonucu bulundu. İçerikleri çekiliyor...");

//                foreach (var resultLink in resultLinks.Take(10)) // İlk 10 sonucu alalım
//                {
//                    var resultResponse = await httpClient.GetAsync(resultLink);
//                    var resultContent = await resultResponse.Content.ReadAsStringAsync();

//                    var resultDocument = new HtmlDocument();
//                    resultDocument.LoadHtml(resultContent);

//                    var title = resultDocument.DocumentNode.SelectSingleNode("//h3[@class ='article-title']")?.InnerText.Trim();
//                    //   var abstractText = resultDocument.DocumentNode.SelectSingleNode("//div[@id='gs_res_ccl_mid']/div[@class='gs_rs']")?.InnerText.Trim();
//                    var dateNode = resultDocument.DocumentNode.SelectSingleNode("//tr[th='Publication Date']/td");
//                    var ozet = resultDocument.DocumentNode.SelectSingleNode("//h3[text()='Abstract']/following-sibling::p");
//                    var keywords = resultDocument.DocumentNode.SelectSingleNode("//h3[text()='Keywords']/following-sibling::p");
//                    var references = resultDocument.DocumentNode.SelectSingleNode("//ul[@class='fa-ul']");
//                    var yayinTuru = resultDocument.DocumentNode.SelectSingleNode("//tr[th='Journal Section']/td");
//                    var authorNodes = resultDocument.DocumentNode.SelectSingleNode("//tr[th = 'Authors']/td/p");

//                    Console.WriteLine($"Başlık: {title}");
//                    // Console.WriteLine($"Özet: {ozet}");
//                    //Console.WriteLine("Yayın Türü " + yayinTuru);
//                    //if (dateNode != null)
//                    //{
//                    //    Console.WriteLine($"Yayınlanma Tarihi : {dateNode}");

//                    //}
//                    if (yayinTuru != null)
//                    {
//                        Console.WriteLine("YAYIN TÜRÜ " + yayinTuru.InnerText);
//                    }
//                    if (dateNode != null)
//                    {
//                        Console.WriteLine("Yayınlanma Tarihi :  " + dateNode.InnerText);
//                    }
//                    if (ozet != null)
//                    {
//                        Console.WriteLine("ÖZET " + ozet.InnerText);
//                    }

//                    if (keywords != null)
//                    {
//                        var key = keywords.SelectNodes(".//a[@href]");
//                        foreach (var hrefElement in key)
//                        {
//                            string hrefValue = hrefElement.InnerText.Trim();
//                            Console.WriteLine("Anahtar Kelime: " + hrefValue);
//                        }
//                    }
//                    if (references != null)
//                    {
//                        var refX = references.SelectNodes(".//li");

//                        foreach (var x in refX)
//                        {

//                            Console.WriteLine("Referans : " + x.InnerText);
//                        }
//                    }

//                    if (authorNodes != null)
//                    {
//                        var yazar = authorNodes.SelectNodes(".//a");
//                        foreach (var name in yazar)
//                        {
//                            Console.WriteLine("YAZAR " + name.InnerText.Trim());
//                        }


//                    }

//                    //Console.WriteLine($"Yazar :{nameElement}");
//                    Console.WriteLine();
//                }
//                Console.WriteLine("\n");
//                Console.WriteLine("\n");
//                Console.WriteLine("\n");
//                Console.WriteLine("\n");


//            }
//            else
//            {
//                Console.WriteLine("Arama sonuçları bulunamadı.");
//            }
//        }

        
        

//        public GetManyResult<AkademikYayin> GetAkademikYayinByName()
//        {
//            throw new NotImplementedException();
//        }

//        public Task EklemeYap()
//        {
//            throw new NotImplementedException();
//        }

//        public Task EklemeYap(string searchString)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}


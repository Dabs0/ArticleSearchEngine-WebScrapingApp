using Business.Abstract;
using Core.Models;
using Core.Repository.Abstract;
using DataAccess.Abstarct;
using Entities.Concrete;
using HtmlAgilityPack;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Nest;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using yazlab2proje1.Models;

namespace Business.Concrete
{
    public class AkademikYayinManager : IAkademikYayinService
    {
        private readonly IAkademikYayinDataAccess _akademikYayinDataAccess;
        private readonly Core.Repository.Abstract.IRepository<AkademikYayin> _AkademikRepository;
        List<string> keywordList = new List<string>();
        List<string> referenceList = new List<string>();
        List<string> yayincilar = new List<string>();
        List<string> yazarlar = new List<string>();
        List<string> aramaAnahtarKelime = new List<string>();
        public List<AkademikYayin> articleList = new List<AkademikYayin>();
        public AkademikYayinManager(IAkademikYayinDataAccess akademikYayinDataAccess, Core.Repository.Abstract.IRepository<AkademikYayin> akademikYayinRepository)
        {
            _akademikYayinDataAccess = akademikYayinDataAccess;
            _AkademikRepository = akademikYayinRepository;
        }
        public IMongoCollection<BsonDocument> getCollection()
        {
            const string connectionUri = "mongodb://localhost:27017/";
            var client = new MongoClient(connectionUri);


            var database = client.GetDatabase("YazLab2");


            var collection = database.GetCollection<BsonDocument>("AkademikYayin");
            return collection;
        }

        public AkademikYayin createModelArticle(BsonDocument article)
        {

            return BsonSerializer.Deserialize<AkademikYayin>(article);
        }

        public ElasticClient getElasticClient()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("your_index_name");
            var elasticClient = new ElasticClient(settings);
            return elasticClient;
        }
        public async Task getDBArticlesAsync()
        {


            // Tüm makaleleri getir
            var filter = new BsonDocument();
            var cursor = await getCollection().FindAsync(filter);
            var articles = await cursor.ToListAsync();

            // Makaleleri Article modeli şeklinde bir listede tut
            articleList.Clear();
            foreach (var article in articles)
            {

                articleList.Add(createModelArticle(article));
            }

            // Listeyi yazdır
            Console.WriteLine("Makale Listesi:");
            foreach (var article in articleList)
            {
                Console.WriteLine($"{article.Id}, {article.Ad}, {string.Join(", ", article.yazars)}");
            }

        }

        //Yayınları id'sine göre al
        public AkademikYayin GetArticleById(ObjectId id)
        {


            // Belirli bir ID'ye göre makaleyi filtrele
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id); // Varsayılan olarak MongoDB'de ID "_id" alanı olarak saklanır

            // Makaleyi bul
            var articleDocument = getCollection().Find(filter).FirstOrDefault();

            if (articleDocument != null)
            {
                // Article modeline dönüştür


                return createModelArticle(articleDocument);
            }

            return null; // Makale bulunamazsa null döndür
        }
        public List<AkademikYayin> getArticleList()
        {
            return articleList;
        }

        //elastic search verisini güncelle
        public void updateElasticSearch()
        {
            // MongoDB'den belgeleri al
            var mongoDocuments = getCollection().Find(Builders<BsonDocument>.Filter.Empty).ToList();

            // MongoDB belgelerini Elasticsearch belgelerine dönüştür ve Elasticsearch'e ekle
            var elasticDocuments = new List<AkademikYayin>();
            foreach (var articleDocument in mongoDocuments)
            {
                elasticDocuments.Add(createModelArticle(articleDocument));
            }


            foreach (var elasticDocument in elasticDocuments)
            {
                var indexResponse = getElasticClient().IndexDocument(elasticDocument);
                if (!indexResponse.IsValid)
                {
                    Console.WriteLine($"Hata: {indexResponse.DebugInformation}");
                }
            }

            Console.WriteLine("Belgeler Elasticsearch'e başarıyla eklendi.");
        }

        //kelimeyi elasticsearch le ara
        public async Task<List<AkademikYayin>> searchEngineAsync(string searchString)
        {
            //webScrapingAsync(searchString).Wait();
            var searchResponse = getElasticClient().Search<AkademikYayin>(s => s
                .Query(q => q
                        .MultiMatch(mm => mm
                        .Query(searchString)
                        .Fields(f => f
                            .Field(ff => ff.Ad)

                        )
                    )
                )
            );
            if(searchResponse.IsValid && searchResponse.Hits.Count >= 10)
            {
                Console.WriteLine("Tüm veriler Veritabanından alınıyor...");
            }
            else
            {
                Console.WriteLine(10 - searchResponse.Hits.Count+ " Tane Arama Veritabanında Eksik. Siteden Alınıyor...");
                await EklemeYap(searchString,10-searchResponse.Hits.Count);
                
                searchResponse = getElasticClient().Search<AkademikYayin>(s => s
                    .Query(q => q
                            .MultiMatch(mm => mm
                            .Query(searchString)
                            .Fields(f => f
                                .Field(ff => ff.Ad)

                            )
                        )
                    )
                );
            }
            
            Console.WriteLine(getElasticClient().Indices);

            if (searchResponse.IsValid)
            {
                var debugInfo = searchResponse.DebugInformation;
                
                Console.WriteLine("debug info: "+debugInfo);
                
                List<AkademikYayin> foundArticles = new List<AkademikYayin>();
                foreach (var hit in searchResponse.Hits)
                {

                    Console.WriteLine($"Belge Id: {hit.Id}, Başlık: {hit.Source.Ad}");

                    foundArticles.Add(hit.Source);
                }
                return foundArticles;
            }
            else
            {
                Console.WriteLine($"Arama sırasında bir hata oluştu: {searchResponse.DebugInformation}");
            }
            return null;
        }



        public async Task EklemeYap(string keyword,int searchCount) 
        {
            string url = $"https://dergipark.org.tr/en/search?q={HttpUtility.UrlEncode(keyword)}";

            // Tarayıcıyı kapatın

            // Alınan URL'yi kullanın veya işleyin


            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);



            var httpClient = new HttpClient(); // get işlemi için oluşturulur
            var response = await httpClient.GetAsync(url); // içerdeki url'ye erişmek için kullanılır
            var content = await response.Content.ReadAsStringAsync(); // yukarıda eriştiğimiz stringi kullanmak için oluşturulur

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            var resultLinks = htmlDocument.DocumentNode
                .SelectNodes("//h5[@class='card-title']/a/@href")
                ?.Select(link => link.GetAttributeValue("href", string.Empty))
                .ToList();

            if (resultLinks != null)
            {
                Console.WriteLine($"Toplam {resultLinks.Count} arama sonucu bulundu. İçerikleri çekiliyor...");

                foreach (var resultLink in resultLinks.Take(20)) // İlk 20 sonucu alalım
                {
                    if (searchCount <= 0)
                        break;
                    keywordList.Clear();
                    referenceList.Clear();
                    yayincilar.Clear();
                    yazarlar.Clear();
                    var resultResponse = await httpClient.GetAsync(resultLink);
                    var resultContent = await resultResponse.Content.ReadAsStringAsync();
                    var resultDocument = new HtmlDocument();
                    resultDocument.LoadHtml(resultContent);
                    var keyx = resultDocument.DocumentNode.SelectSingleNode("//h3[text()='Keywords']/following-sibling::p");
                    var references = resultDocument.DocumentNode.SelectSingleNode("//ul[@class='fa-ul']");
                    var yayinTuru = resultDocument.DocumentNode.SelectSingleNode("//tr[th='Journal Section']/td");
                    var tableRows = resultDocument.DocumentNode.SelectSingleNode("//table[@class='table table-striped m-table cite-table']/tbody");
                    var authorSectionNode = resultDocument.DocumentNode.SelectSingleNode("//th[text() = 'Authors']");

                    var imgElement = resultDocument.DocumentNode.SelectSingleNode("//img[contains(@class,'d-flex') and contains(@class,'justify-content-center') and contains(@class,'rounded') and contains(@class,'mx-auto') and contains(@class,'d-block')]");

                    // img etiketinin src özelliğini al
                    string imgSrc = imgElement?.GetAttributeValue("src", "");

                    if (authorSectionNode != null)
                    {
                        var tdNode = authorSectionNode.SelectSingleNode(".//following-sibling::td");

                        if (tdNode != null)
                        {
                            var spans = tdNode.SelectNodes(".//p/span[@style='font-weight: 600']");

                            if (spans != null && spans.Count > 0)
                            {
                                foreach (var spanNode in spans)
                                {
                                    string text = spanNode.InnerText.Trim();
                                    string isim = text.Substring(0, text.IndexOf(" This is me")).Trim();
                                    // İlgili metni kullanın veya işleyin
                                    yazarlar.Add(isim);

                                }
                            }
                            var sp2 = tdNode.SelectNodes(".//p//a[@style='font-weight: 600']");

                            if (sp2 != null)
                            {
                                if (sp2.Count > 0)
                                {
                                    foreach (var sp in sp2)
                                    {
                                        string text = sp.InnerText.Trim();
                                        await Console.Out.WriteLineAsync(text);
                                        yazarlar.Add(text);
                                    }

                                }

                            }
                        }
                    }

                    if (tableRows != null)
                    {
                        var table = tableRows.SelectNodes(".//tr");
                        foreach (var row in table)
                        {
                            yayincilar.Add(row.SelectSingleNode(".//td[2]")?.InnerText.Trim());
                        }
                    }

                    if (keyx != null)
                    {

                        var key = keyx.SelectNodes(".//a[@href]");
                        foreach (var hrefElement in key)
                        {
                            string hrefValue = hrefElement.InnerText.Trim();
                            keywordList.Add(hrefValue);
                        }
                    }
                    if (references != null)
                    {

                        var ref1 = references.SelectNodes(".//li");
                        if (ref1 != null)
                        {
                            foreach (var hrefElement in ref1)
                            {
                                string hrefValue = hrefElement.InnerText;
                                referenceList.Add(hrefValue);
                            }
                        }

                    }
                    
                    AkademikYayin yayin= new AkademikYayin
                    {
                        urlAdresi = resultLink,
                        Ad = resultDocument.DocumentNode.SelectSingleNode("//h3[@class ='article-title']")?.InnerText.Trim(),
                        yayinlanmaTarihi = resultDocument.DocumentNode.SelectSingleNode("//tr[th='Publication Date']/td")?.InnerText,
                        //yayinTarihi = DateTime.ParseExact(resultDocument.DocumentNode.SelectSingleNode("//tr[th='Publication Date']/td")?.InnerText, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        ozet = resultDocument.DocumentNode.SelectSingleNode("//h3[text()='Abstract']/following-sibling::p")?.InnerText,
                        anahtarKelimes = keywordList.Count > 0
                         ? keywordList.Select(keyw => new AnahtarKelime
                         {
                             anahtarKelimeId = ObjectId.GenerateNewId(),
                             kelimeAdi = keyw
                         }).ToList()
                        : new List<AnahtarKelime>(),
                        Referans = referenceList.Count > 0
                         ? referenceList.Select(ref2 => new Entities.Concrete.Reference
                         {
                             referenceId = ObjectId.GenerateNewId(),
                             referenceAd = ref2
                         }).ToList()
                         : new List<Entities.Concrete.Reference>(),

                        yayinTurus = yayinTuru != null
                         ? new YayinTürü
                         {
                             yayinTürüId = ObjectId.GenerateNewId(),
                             YayinTürüAd = yayinTuru.InnerText
                         }
                         : null,
                        yayincilars = yayincilar.Count > 0
                         ? referenceList.Select(ref2 => new Yayıncilar
                         {
                             yayinciId = ObjectId.GenerateNewId(),
                             yayinciAd = ref2
                         }).ToList()
                         : new List<Yayıncilar>(),

                        yazars = yazarlar.Count > 0
                         ? yazarlar.Select(ref2 => new Yazar
                         {
                             yazarId = ObjectId.GenerateNewId(),
                             yazarAdSoyad = ref2
                         }).ToList()
                         : new List<Yazar>(),
                        image = imgSrc


                    };
                    if (!checkIfExists(yayin))
                    {
                        var resultInsert = _AkademikRepository.InsertOneAsync(yayin);
                        Console.WriteLine("EKLEME YAPILDI");
                        searchCount--;
                    }
                    else
                    {
                        Console.WriteLine("Zaten Mevcut");
                    }
                    
                    





                   
                }
            }
            updateElasticSearch();
        
        }
        public bool checkIfExists(AkademikYayin article)
        {
            foreach (AkademikYayin x in articleList)
            {
                if (article.urlAdresi.Equals(x.urlAdresi))
                {
                    return true;
                }
            }
            
            return false;
        }

        
    }

    //public GetManyResult<AkademikYayin> GetAkademikYayinByName()
    //{
    //    var result = new GetManyResult<AkademikYayin>();

    //    //try
    //    //{
    //    //    var yayinList = _akademikYayinDataAccess.GetAll();
    //    //    yayinList= yayinList.OrderBy(x => x.Ad).ToList();
    //    //    result = yayinList;
    //    //}
    //    //catch (Exception ex)
    //    //{
    //    //    result.Success = false;
    //    //    result.Message = "Bir hata oluştu: " + ex.Message;
    //    //}

    //    return result;
    //}



}

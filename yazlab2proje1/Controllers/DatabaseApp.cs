using MongoDB.Bson;
using MongoDB.Driver;
using Nest;
using yazlab2proje1.Models;

namespace yazlab2proje1.Controllers
{
    public class DatabaseApp
    {
       
        public List<Article> articleList = new List<Article>();
        public IMongoCollection<BsonDocument> getCollection()
        {
            const string connectionUri = "mongodb://localhost:27017/";
            var client = new MongoClient(connectionUri);

            // Veritabanını seç
            var database = client.GetDatabase("local");

            // Koleksiyonu seç
            var collection = database.GetCollection<BsonDocument>("webscraping");
            return collection;
        }
        public ElasticClient getElasticClient()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("webscraping_index");
            var elasticClient = new ElasticClient(settings);
            return elasticClient;
        }
        public Article createModelArticle(BsonDocument article)
        {
            return new Article
            {
                objectId = article.GetValue("_id").AsObjectId, // "_id" alanı integer olarak kabul edilmiş olsun
                Id = article.GetValue("Id").AsInt32, // "_id" alanı integer olarak kabul edilmiş olsun
                title = article.GetValue("title").AsString,
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
        }
        public async Task getArticlesAsync()
        {


            // Tüm makaleleri getir
            var filter = new BsonDocument();
            var cursor = await getCollection().FindAsync(filter);
            var articles = await cursor.ToListAsync();

            // Makaleleri Article modeli şeklinde bir listede tut

            foreach (var article in articles)
            {
                
                articleList.Add(createModelArticle(article));
            }

            // Listeyi yazdır
            Console.WriteLine("Makale Listesi:");
            foreach (var article in articleList)
            {
                Console.WriteLine($"{article.Id}, {article.title}, {string.Join(", ", article.authors)}");
            }

        }
        public Article GetArticleById(ObjectId id)
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
        public void updateElasticSearch()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            var mongoDatabase = mongoClient.GetDatabase("local");
            var mongoCollection = mongoDatabase.GetCollection<BsonDocument>("webscraping");

            // Elasticsearch bağlantısını yapılandırın
           

            // MongoDB'den belgeleri alın
            var mongoDocuments = getCollection().Find(Builders<BsonDocument>.Filter.Empty).ToList();

            // MongoDB belgelerini Elasticsearch belgelerine dönüştürün ve Elasticsearch'e ekleyin
            var elasticDocuments = new List<Article>();
            foreach (var articleDocument in mongoDocuments)
            {
                elasticDocuments.Add(createModelArticle(articleDocument));
            }

            // Elasticsearch belgelerini Elasticsearch'e ekleyin
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
        public List<Article> searchEngine(string searchString)
        {
            var searchResponse = getElasticClient().Search<Article>(s => s
    .Query(q => q
            .MultiMatch(mm => mm
            .Query(searchString)
            .Fields(f => f
                .Field(ff => ff.title)
                .Field(ff => ff.authors)
                .Field(ff => ff.type)
                .Field(ff => ff.publisher)
                .Field(ff => ff.keywords)
                .Field(ff => ff.summary)
                .Field(ff => ff.refs)
                .Field(ff => ff.doi)
                .Field(ff => ff.url)
            )
        )
    )
);
            
            if (searchResponse.IsValid)
            {
                List<Article> foundArticles = new List<Article>();
                foreach (var hit in searchResponse.Hits)
                {
                    
                    Console.WriteLine($"Belge Id: {hit.Id}, Başlık: {hit.Source.title}");
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
    }
}


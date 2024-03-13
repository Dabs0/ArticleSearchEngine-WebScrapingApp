using MongoDB.Bson;

namespace yazlab2proje1.Models
{
    public class Article
    {
        public ObjectId objectId { get; set; }
        public int Id { get; set; }
        public string title { get; set; }
        public string[] authors { get; set; }
        public string  type { get; set; }
        public DateTime publishDate { get; set; }
        public string publisher { get; set; }
        public string[] keywords { get; set; }
        public string summary { get; set; }
        public string[] refs { get; set; }
        public int citNumber { get; set; }
        public string doi { get; set; }
        public string url { get; set; }
    }
}

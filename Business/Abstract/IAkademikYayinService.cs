using Core.Models;
using Entities.Concrete;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using yazlab2proje1.Models;

namespace Business.Abstract
{
    public interface IAkademikYayinService
    {
        //GetManyResult<AkademikYayin> GetAkademikYayinByName();
        Task EklemeYap(string searchString);
        public void updateElasticSearch();
        public Task getArticlesAsync();
        public Article GetArticleById(ObjectId id);
        public List<Article> getArticleList();
        public List<Article> searchEngine(string searchString);
        
    }
}

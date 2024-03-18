using Core.Models;
using Entities.Concrete;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Business.Abstract
{
    public interface IAkademikYayinService
    {
        //GetManyResult<AkademikYayin> GetAkademikYayinByName();
        Task EklemeYap(string searchString,int searchCount);
        public Task updateElasticSearch();

		public Task getDBArticlesAsync();
        public AkademikYayin GetArticleById(ObjectId id);
        public List<AkademikYayin> getArticleList();
        public Task<List<AkademikYayin>> searchEngineAsync(string searchString);
        public void checkIfYayinTuruExists(YayinTuru yayinTuru);
        public List<YayinTuru> getYayinTurleriList();
        public Task getYayinTurleriAsync();






	}
}

using Core.Settings;
using DataAccess.Abstarct;
using DataAccess.Context;
using DataAccess.Repository;
using Entities.Concrete;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete
{
    public class AkademikYayinDataAccess : MongoRepositoryBase<AkademikYayin>, IAkademikYayinDataAccess
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<AkademikYayin> _collection;
        public AkademikYayinDataAccess(IOptions<MongoSettings> settings) : base(settings)
        {
            _context = new MongoDbContext(settings);
            _collection = _context.GetCollection<AkademikYayin>();
        }

        public List<AkademikYayin> isimSirasinaGoreGetir()
        {
            throw new NotImplementedException();
        }
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class YayinTuru
    {
        [BsonId]
        public ObjectId yayinTuruId { get; set; }

        public string YayinTuruAd { get; set; }
    }
}

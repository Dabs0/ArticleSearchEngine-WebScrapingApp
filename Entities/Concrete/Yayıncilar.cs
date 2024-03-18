using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class Yayincilar
    {
        [BsonId]
        public ObjectId yayinciId { get; set; }
        public string yayinciAd { get; set; }
    }
}

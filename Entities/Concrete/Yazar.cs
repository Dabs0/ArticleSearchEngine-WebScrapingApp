using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class Yazar
    {
        [BsonId]
        public ObjectId yazarId { get; set; }
        public string yazarAdSoyad { get; set; }

    }
}

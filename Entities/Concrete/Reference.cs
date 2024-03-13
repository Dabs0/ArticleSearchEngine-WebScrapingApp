using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class Reference
    {
        [BsonId]
        public ObjectId referenceId { get; set; }
        public string referenceAd { get; set; }
    }
}

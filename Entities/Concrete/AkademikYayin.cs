    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Entities.Concrete
    {
        public class AkademikYayin
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public string Ad { get; set; }
            public ICollection<Yazar> yazars { get; set; }
            public string yayinlanmaTarihi { get; set; }
        public DateTime yayinTarihi { get; set; }

        public ICollection<Yayıncilar>? yayincilars { get; set; }
            public string aramaAnahtarKelime { get; set; }

            public ICollection<AnahtarKelime> anahtarKelimes { get; set; }

            public YayinTürü yayinTurus { get; set; }
            public string ozet { get; set; }
            public ICollection<Reference> Referans { get; set; }
            public int? alintiSayisi { get; set; }
            public string urlAdresi { get; set; }

        public string image { get; set; }

    }
    }

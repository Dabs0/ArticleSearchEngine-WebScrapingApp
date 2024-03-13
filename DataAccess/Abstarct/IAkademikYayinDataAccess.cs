using Core.Repository.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstarct
{
    public interface IAkademikYayinDataAccess : IRepository<AkademikYayin>
    {
       public List<AkademikYayin> isimSirasinaGoreGetir(); 
        
    }
}

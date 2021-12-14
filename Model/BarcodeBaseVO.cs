using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 高并发下生成单据号.Model
{
    public class BarcodeBaseVO
    {
       public string ID { get; set; }

       public string PREFIX { get; set; }

       public string CURRENT_VALUE { get; set; }
    }
}

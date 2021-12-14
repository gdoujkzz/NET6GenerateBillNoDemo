using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 高并发下生成单据号.Entity
{
    public class OrgModelBase
    {
        public string ENTERPRISE_ID { get; set; }


        public string ORG_ID { get; set; }



        public string ID { get; set; }



        public string USER_CREATED { get; set; }



        public string USER_MODIFIED { get; set; }


        public DateTime? DATETIME_CREATED { get; set; }



        public DateTime? DATETIME_MODIFIED { get; set; }


        public string STATE { get; set; }
    }
}

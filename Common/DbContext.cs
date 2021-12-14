using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 高并发下生成单据号.Common
{
    public class DbContext
    {
        public static SqlSugarClient GetInstance()
        {


            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["FrameworkConnection"].ToString(),//连接符字串
                DbType = DbType.Oracle,
            });

            //db.Aop.OnLogExecuting = (sql, pars) =>
            //{
            //    var s = sql;
            //    pars.ToList().ForEach(x => sql = sql.Replace(x.ParameterName, "'" + x.Value + "'"));
            //    var p = db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value));
            //    Console.WriteLine(sql);

            //};
            return db;
        }
    }
}

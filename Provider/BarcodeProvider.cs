using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 高并发下生成单据号.Common;
using 高并发下生成单据号.Entity;

namespace 高并发下生成单据号.Provider
{
    public class BarcodeProvider
    {

        public static DateTime GetDbNow()
        {
            using (var db = DbContext.GetInstance())
            {
                return db.GetDate();
            }
        }


        /// <summary>
        /// 一般常见的生成单号做法
        /// </summary>
        /// <param name="billTypeCode"></param>
        /// <param name="OrgId"></param>
        /// <param name="EnterpriseId"></param>
        /// <param name="CurrentUserName"></param>
        /// <returns></returns>
        public static string GenerateBillNo(string billTypeCode, string OrgId, string EnterpriseId, string CurrentUserName)
        {
            using (var db = DbContext.GetInstance())
            {
                DateTime dbTime = DateTime.Now;
                var prefix = billTypeCode + DateTime.Now.ToString("yyyyMMdd");
                string barcodeCategory = billTypeCode + "TEST_BILL_CATEGORY";

                string newBillNo = prefix + "0001";
                //当前序号
                var currentSeq = db.Queryable<SFC_BARCODE_SEQUENCE>()
                    .Where(x => x.BARCODE_CATEGORY == barcodeCategory && x.PREFIX == prefix)
                    .Where(x => x.STATE == "A" && x.ORG_ID == OrgId && x.ENTERPRISE_ID == EnterpriseId)
                    .ToList()
                    .FirstOrDefault();
                if (currentSeq == null)
                {
                    SFC_BARCODE_SEQUENCE model = new SFC_BARCODE_SEQUENCE();
                    model.ID = Guid.NewGuid().ToString("N").ToUpper();
                    model.DATETIME_CREATED = dbTime;
                    model.USER_CREATED = CurrentUserName;
                    model.STATE = "A";
                    model.ORG_ID = OrgId;
                    model.ENTERPRISE_ID = EnterpriseId;
                    model.BARCODE_CATEGORY = barcodeCategory;
                    model.PREFIX = prefix;
                    model.CURRENT_VALUE = 1;
                    model.BARCODE_RULE = $"检验单据类型（{billTypeCode}） + 年月日(yyyyMMdd) + 4位流水";

                    db.Insertable(model).ExecuteCommand();
                }
                else
                {
                    db.Updateable<SFC_BARCODE_SEQUENCE>()
                        .Where(x => x.ID == currentSeq.ID)
                        .SetColumns(t => new SFC_BARCODE_SEQUENCE()
                        {
                            USER_MODIFIED = CurrentUserName,
                            DATETIME_MODIFIED = dbTime,
                            CURRENT_VALUE = (currentSeq.CURRENT_VALUE + 1)
                        }).ExecuteCommand();

                    newBillNo = prefix + (currentSeq.CURRENT_VALUE + 1).ToString().PadLeft(4, '0');
                }
                return newBillNo;
            }
        }


        /// <summary>
        /// 利用数据库事务实现的悲观锁做法
        /// </summary>
        /// <param name="billTypeCode"></param>
        /// <param name="OrgId"></param>
        /// <param name="EnterpriseId"></param>
        /// <param name="CurrentUserName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GenerateBillNoByPessimisticLock(string billTypeCode, string OrgId, string EnterpriseId, string CurrentUserName)
        {
            using (var db = DbContext.GetInstance())
            {
                var prefix = billTypeCode + DateTime.Now.ToString("yyyyMMdd");
                string barcodeCategory = billTypeCode + "TEST_BILL_CATEGORY";

                string newbillNo = prefix + "0001";
                var ids = db.Queryable<SFC_BARCODE_SEQUENCE>().Where(v => v.PREFIX == prefix && v.ORG_ID == OrgId)
                    .Select(v => v.ID).ToList();

                if (ids.Count > 1) throw new Exception($"单据前缀重复{prefix}");
                if (ids.Count <= 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            SFC_BARCODE_SEQUENCE model = new SFC_BARCODE_SEQUENCE();
                            model.ID = System.Guid.NewGuid().ToString("N").ToUpper();
                            model.DATETIME_CREATED = DateTime.Now;
                            model.USER_CREATED = CurrentUserName;
                            model.ENTERPRISE_ID = "*";
                            model.PREFIX = prefix;
                            model.ORG_ID = OrgId;
                            model.CURRENT_VALUE = 0;
                            model.BARCODE_CATEGORY = barcodeCategory;
                            model.STATE = "A";
                            model.BARCODE_RULE = $"检验单据类型（{billTypeCode}）+年月日(yyyyMMdd) + 4位流水";
                            db.Insertable(model).ExecuteCommand();
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(200);
                        }

                        ids = db.Queryable<SFC_BARCODE_SEQUENCE>().Where(v => v.PREFIX == prefix && v.ORG_ID == OrgId)
                            .Select(v => v.ID).ToList();

                        if (ids.Count > 0) break;
                        Thread.Sleep(300);
                    }
                    if (ids.Count <= 0) throw new Exception($"插入单据前缀失败{prefix}");
                }

                var id = ids.FirstOrDefault();
                int seq = 0;
                try
                {
                    db.Ado.BeginTran();

                    db.Updateable<SFC_BARCODE_SEQUENCE>().Where(t => t.ID == id)
                                            .SetColumns(t => new SFC_BARCODE_SEQUENCE()
                                            {

                                                USER_MODIFIED = CurrentUserName,
                                                DATETIME_MODIFIED = DateTime.Now,
                                                CURRENT_VALUE = t.CURRENT_VALUE + 1
                                            }).ExecuteCommand();

                    //同一个事务，是可以见的。
                    seq = db.Queryable<SFC_BARCODE_SEQUENCE>().Where(v => v.ID == id)
                          .Select(v => v.CURRENT_VALUE).ToList().FirstOrDefault();

                    db.Ado.CommitTran();
                }
                catch (Exception ex)
                {
                    db.Ado.RollbackTran();
                    throw;
                }
                return prefix + Convert.ToString(seq).PadLeft(4, '0');
            }
        }



        /// <summary>
        /// 利用类似于版本号实现的乐观锁
        /// </summary>
        /// <param name="billTypeCode"></param>
        /// <param name="OrgId"></param>
        /// <param name="EnterpriseId"></param>
        /// <param name="CurrentUserName"></param>
        /// <returns></returns>
        public static string GenerateBillNoByOptimisticLock(string billTypeCode, string OrgId, string EnterpriseId, string CurrentUserName)
        {
            using (var db = DbContext.GetInstance())
            {
                var prefix = billTypeCode + DateTime.Now.ToString("yyyyMMdd");
                string barcodeCategory = billTypeCode + "TEST_BILL_CATEGORY";

                string newBillNo = prefix + "0001";

                var sugarParameters = new List<SugarParameter>();

                #region 参数化查询SQL

                string sql = @"SELECT 
                      BARCODE_CATEGORY,
                      PREFIX,
                      CURRENT_VALUE,
                      BARCODE_RULE,
                      ENTERPRISE_ID,
                      ORG_ID,
                      ID,
                      USER_CREATED,
                      USER_MODIFIED,
                      DATETIME_CREATED,
                      DATETIME_MODIFIED,
                      STATE,
                      ROW_NUMBER() OVER(ORDER BY sysdate) AS RowIndex
                 FROM SFC_BARCODE_SEQUENCE bmq
                WHERE 1 = 1
                  and STATE = @STATE
                  and ENTERPRISE_ID =@ENTERPRISE_ID
                  AND PREFIX = @PREFIX
                  AND ORG_ID = @ORG_ID
                  AND BARCODE_CATEGORY = @BARCODE_CATEGORY";

                sugarParameters.Add(new SugarParameter("@STATE", "A"));
                sugarParameters.Add(new SugarParameter("@ENTERPRISE_ID", "*"));
                sugarParameters.Add(new SugarParameter("@PREFIX", prefix));
                sugarParameters.Add(new SugarParameter("@ORG_ID", OrgId));
                sugarParameters.Add(new SugarParameter("@BARCODE_CATEGORY", barcodeCategory));

                var currentSeq = db.Ado.SqlQuery<SFC_BARCODE_SEQUENCE>(sql,sugarParameters).FirstOrDefault();
                #endregion
                if (currentSeq == null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            SFC_BARCODE_SEQUENCE model = new SFC_BARCODE_SEQUENCE();
                            model.ID = Guid.NewGuid().ToString("N").ToUpper();
                            model.DATETIME_CREATED = DateTime.Now;
                            model.USER_CREATED = CurrentUserName;
                            model.STATE = "A";
                            model.ORG_ID = OrgId;
                            model.ENTERPRISE_ID = EnterpriseId;
                            model.BARCODE_CATEGORY = barcodeCategory;
                            model.PREFIX = prefix;
                            model.CURRENT_VALUE = 0;
                            model.BARCODE_RULE = $"检验单据类型（{billTypeCode}） + 年月日(yyyyMMdd) + 4位流水";
                            db.Insertable(model).ExecuteCommand();
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("出了异常");
                            Thread.Sleep(200);
                        }
                        //再查查
                        currentSeq = db.Ado.SqlQuery<SFC_BARCODE_SEQUENCE>(sql, sugarParameters).SingleOrDefault();
                        if (currentSeq != null) {
                            break;
                        }
                        else
                        {
                           Thread.Sleep(300);
                        };
                    }
                }

                
                var effectRowsCount = db.Updateable<SFC_BARCODE_SEQUENCE>()
                        .Where(x => x.ID == currentSeq.ID && x.CURRENT_VALUE == currentSeq.CURRENT_VALUE)
                        .SetColumns(t => new SFC_BARCODE_SEQUENCE()
                        {
                            USER_MODIFIED = CurrentUserName,
                            DATETIME_MODIFIED = DateTime.Now,
                            CURRENT_VALUE = (currentSeq.CURRENT_VALUE + 1)
                        }).ExecuteCommand();
                if (effectRowsCount > 0)
                {
                    newBillNo = prefix + (currentSeq.CURRENT_VALUE + 1).ToString().PadLeft(4, '0');
                }
                else
                {
                    newBillNo = "获取单号失败，请重新获取";
                }
                return newBillNo;
            }
        }
    }
}

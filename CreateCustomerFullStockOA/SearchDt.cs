using System;
using System.Data;
using System.Data.SqlClient;

namespace CreateCustomerFullStockOA
{
    //查询
    public class SearchDt
    {
        ConDb conDb = new ConDb();
        SqlList sqlList = new SqlList();

        /// <summary>
        /// 根据SQL语句查询得出对应的DT
        /// </summary>
        /// <param name="conid">0:连接K3 1:连接OA</param>
        /// <param name="sqlscript"></param>
        /// <returns></returns>
        private DataTable UseSqlSearchIntoDt(int conid, string sqlscript)
        {
            var resultdt = new DataTable();

            try
            {
                var sqlDataAdapter = new SqlDataAdapter(sqlscript, conDb.GetK3CloudConn(conid));
                sqlDataAdapter.Fill(resultdt);
            }
            catch (Exception)
            {
                resultdt.Rows.Clear();
                resultdt.Columns.Clear();
            }

            return resultdt;
        }

        /// <summary>
        /// 按照指定的SQL语句执行记录(更新时使用)
        /// </summary>
        private void Generdt(string sqlscript)
        {
            using (var sql = conDb.GetK3CloudConn(1))
            {
                sql.Open();
                var sqlCommand = new SqlCommand(sqlscript, sql);
                sqlCommand.ExecuteNonQuery();
                sql.Close();
            }
        }

        /// <summary>
        /// 发货通知单相关信息获取
        /// </summary>
        /// <returns></returns>
        public DataTable SearchDeliveryNotice(string orderno)
        {
            var dt = UseSqlSearchIntoDt(0, sqlList.SearchDeliveryNotice(orderno));
            return dt;
        }

        /// <summary>
        /// 根据客户ID查询相关信息
        /// </summary>
        /// <param name="custid"></param>
        /// <returns></returns>
        public DataTable SearchCustomerInfo(int custid)
        {
            var dt = UseSqlSearchIntoDt(0, sqlList.SearchCustomerInfo(custid));
            return dt;
        }

        /// <summary>
        /// 根据客户ID获取收款单相关信息
        /// </summary>
        /// <param name="custid"></param>
        /// <returns></returns>
        public DataTable SearchReciveBillInfo(int custid)
        {
            var dt = UseSqlSearchIntoDt(0, sqlList.SearchReciveBillInfo(custid));
            return dt;
        }

        /// <summary>
        /// 根据客户ID获取应收单相关信息
        /// </summary>
        /// <param name="custid"></param>
        /// <returns></returns>
        public decimal SearchReceivableInfo(int custid)
        {
            var result = Convert.ToDecimal(UseSqlSearchIntoDt(0, sqlList.SearchReceivableInfo(custid)).Rows[0][0]);
            return result;
        }

        /// <summary>
        /// 检查客户是否有信用额度,有才将客户的所有相关信息插入(no need)
        /// </summary>
        /// <param name="custid"></param>
        /// <returns></returns>
        public DataTable CheckCustAccount(int custid)
        {
            var dt = UseSqlSearchIntoDt(0, sqlList.CheckCustAccount(custid));
            return dt;
        }

        /// <summary>
        /// 根据用户名称获取OA-用户ID及部门ID信息
        /// OA使用
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataTable SearchOaInfo(string username)
        {
            var dt = UseSqlSearchIntoDt(1, sqlList.SearchOaInfo(username));
            return dt;
        }

        /// <summary>
        /// 根据发货通知单-客户-获取FISCREDITCHECK (启用信息管理) 0：否 1：是
        /// </summary>
        /// <param name="orderno"></param>
        /// <returns></returns>
        public DataTable CheckisOpen(string orderno)
        {
            var dt = UseSqlSearchIntoDt(0, sqlList.CheckisOpen(orderno));
            return dt;
        }

        /// <summary>
        /// 根据指定条件获取‘各事业部风险账报表’记录
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="scustcode"></param>
        /// <param name="ecustcode"></param>
        /// <returns></returns>
        public DataTable SearchAmount(string sdt, string edt, string scustcode, string ecustcode)
        {
            var dt = UseSqlSearchIntoDt(0, sqlList.SearchAmount(sdt, edt, scustcode, ecustcode));
            return dt;
        }

        /// <summary>
        /// 最后在创建requestid后,对指定记录进行更新记录
        /// </summary>
        /// <param name="requestid"></param>
        /// <param name="valuelist"></param>
        public void UpdateRecord(string requestid, string valuelist)
        {
            var sqllist = sqlList.UpdateRecord(requestid, valuelist);
            Generdt(sqllist);
        }

    }
}

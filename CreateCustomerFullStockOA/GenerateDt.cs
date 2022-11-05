using System;
using System.Data;
using CreateCustomerFullStockOA.WebReference;

namespace CreateCustomerFullStockOA
{
    //运算
    public class GenerateDt
    {
        SearchDt searchDt=new SearchDt();
        Tempdt tempdt=new Tempdt();

        /// <summary>
        /// 获取相关信息,并将K3信息通过OA接口传输至OA,最后达到创建新流程目的
        /// </summary>
        /// <param name="orderno">发货通知单号</param>
        /// <param name="username">记录K3登录用户ID,创建流程时使用</param>
        /// <returns></returns>
        public string GetMessageIntoOa(string orderno,string username)
        {
            var result = "Finish";
            var custDt = new DataTable();            //收集'客户'记录表 
            var receivebillDt = new DataTable();    //收集‘收款单’记录表
            decimal receiveable = 0;               //收集‘应收单’记录
            //var custAccount = new DataTable();    //收集‘客户信用额度’记录

            try
            {
                //根据fcustid判断客户是否勾选‘启用信息管理’ 0：否 1：是
                var checkdt = searchDt.CheckisOpen(orderno).Copy();

                if (Convert.ToInt32(checkdt.Rows[0][1]) == 0)
                {
                    result = "发起流程失败,该客户没有启用‘信息管理’,不作下推OA流程处理";
                }
                else
                {
                    //获取临时表-新增OA流程使用
                    var oatempdt = tempdt.InsertOaRecord();

                    //根据orderno获取‘发货通知单’信息
                    var noticeDt = searchDt.SearchDeliveryNotice(orderno).Copy();

                    //根据‘发货通知单’ 中的‘单据日期’及‘客户代码’为条件,获取‘各事业部风险账报表’中的‘期末余额’及‘超额’记录
                    var amountdt = searchDt.SearchAmount(Convert.ToString(noticeDt.Rows[0][11]),Convert.ToString(noticeDt.Rows[0][11]), 
                                                        Convert.ToString(noticeDt.Rows[0][12]),Convert.ToString(noticeDt.Rows[0][12]));

                    //根据发货通知单‘销售经理’名称获取对应OA人员ID
                    var salesnameOaDt = searchDt.SearchOaInfo(Convert.ToString(noticeDt.Rows[0][9])).Copy();

                    //根据发货通知单.创建人名称获取对应OA人员ID信息
                    var createnameOaDt = searchDt.SearchOaInfo(Convert.ToString(noticeDt.Rows[0][10])).Copy();

                    //根据username获取OA-人员ID 及 部门ID
                    var oaDt = searchDt.SearchOaInfo(username).Copy();

                    //根据noticeDt中的custid获取‘客户’信息
                    if (noticeDt.Rows.Count > 0)
                        custDt = searchDt.SearchCustomerInfo(Convert.ToInt32(noticeDt.Rows[0][0])).Copy();

                    //根据noticeDt中的custid获取‘收款单’信息
                    if (noticeDt.Rows.Count > 0)
                        receivebillDt = searchDt.SearchReciveBillInfo(Convert.ToInt32(noticeDt.Rows[0][0])).Copy();

                    //根据noticeDt中的custid获取'应收单'信息
                    if (noticeDt.Rows.Count > 0)
                        receiveable = searchDt.SearchReceivableInfo(Convert.ToInt32(noticeDt.Rows[0][0]));

                    //检查客户是否有信用额度,有才将客户的所有相关信息插入(不需要)
                    //if (noticeDt.Rows.Count > 0)
                    //    custAccount = searchDt.CheckCustAccount(Convert.ToInt32(noticeDt.Rows[0][0]));

                    //将以上收集的信息插入至oatempdt内
                    oatempdt.Merge(InsertDtIntoTemp(oatempdt, noticeDt, salesnameOaDt, createnameOaDt, custDt, 
                                    receivebillDt, receiveable,amountdt));

                    //对oatempdt表进行数据处理,便于在最后更新时使用
                    var updatelist = GetUpdateList(oatempdt);

                    //将oatempdt数据作为OA接口进行输出,并最后执行OA API方法
                    var resultvalue = CreateOaWorkFlow(Convert.ToInt32(oaDt.Rows[0][0]), updatelist);

                    result = resultvalue == "Finish" ? "Finish" : $"生成OA-超额客户出货流程导常,请联系管理员";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 根据整合后的temp,循环获取相关值,便于在最后获取节点后更新使用
        /// </summary>
        /// <param name="sourcedt"></param>
        /// <returns></returns>
        private string GetUpdateList(DataTable sourcedt)
        {
            var flistid = string.Empty;

            for (var i = 0; i < sourcedt.Columns.Count; i++)
            {
                if (string.IsNullOrEmpty(flistid))
                {
                    flistid = sourcedt.Columns[i].ColumnName + "=" + "'" + Convert.ToString(sourcedt.Rows[0][i]) + "'";
                }
                else
                {
                    flistid += "," + sourcedt.Columns[i].ColumnName + "=" + "'" + Convert.ToString(sourcedt.Rows[0][i]) + "'";
                }
            }
            return flistid;
        }

        /// <summary>
        /// 将相关记录插入至临时表内
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="noticedt">发货通知单临时表</param>
        /// <param name="salesnameOaDt">‘销售经理’名称获取对应OA人员ID</param>
        /// <param name="createnameOaDt">'创建人名称'获取对应OA人员ID信息</param>
        /// <param name="custdt">客户临时表</param>
        /// <param name="receivebillDt">收款单临时表</param>
        /// <param name="receiveable">应收单临时表</param>
        /// <param name="amountdt">根据指定条件获取‘各事业部风险账报表’记录</param>
        /// <returns></returns>
        private DataTable InsertDtIntoTemp(DataTable dt,DataTable noticedt,DataTable salesnameOaDt,DataTable createnameOaDt
                                           , DataTable custdt, DataTable receivebillDt, decimal receiveable,DataTable amountdt)
        {
            decimal amount; //记录出货后超出信用额度欠款(元)

            //将相关值插入至tempdt表内
            var newrow = dt.NewRow();
            newrow[0] = salesnameOaDt.Rows.Count > 0 ? Convert.ToInt32(salesnameOaDt.Rows[0][0]) : 0;                   //申请人(销售经理)   --来源:OA临时表
            newrow[1] = createnameOaDt.Rows.Count > 0 ? Convert.ToString(createnameOaDt.Rows[0][4]) : "";               //申请日期 --来源:OA临时表
            newrow[2] = createnameOaDt.Rows.Count > 0 ? Convert.ToInt32(createnameOaDt.Rows[0][2]) : 0;                 //申请部门  --来源:OA临时表
            newrow[3] = salesnameOaDt.Rows.Count > 0 ? Convert.ToInt32(salesnameOaDt.Rows[0][3]) : 0;                   //岗位      --来源:OA临时表 change date:20221026 ‘岗位’来源‘销售经理’OA
            newrow[23] = createnameOaDt.Rows.Count > 0 ? Convert.ToInt32(createnameOaDt.Rows[0][0]) : 0;                //change date:20221025 设置代办人为‘发货通知单’创建人
            newrow[24] = salesnameOaDt.Rows.Count > 0 ? Convert.ToInt32(salesnameOaDt.Rows[0][2]) : 0; ;                //change date:20221026 设置‘所属大区’来源：销售经理OA

            //chanage date:20221027 客户ID 取‘发货通知单’-‘客户ID’
            newrow[4] = custdt.Rows.Count > 0 ? Convert.ToString(custdt.Rows[0][0]) : "";             //客户ID  --来源:custdt
            newrow[5] = custdt.Rows.Count > 0 ? Convert.ToString(custdt.Rows[0][2]) : "";             //客户名称  --来源:custdt
            newrow[6] = noticedt.Rows.Count > 0 ? Convert.ToDecimal(noticedt.Rows[0][3]) : 0;         //当前信用额度(元) --来源:noticedt *
            newrow[7] = custdt.Rows.Count > 0 ? Convert.ToString(custdt.Rows[0][3]) : "";             //经销商名称(营业执照为准) --来源:custdt
            newrow[8] = custdt.Rows.Count > 0 ? Convert.ToString(custdt.Rows[0][4]) : "";             //法人姓名 --来源:custdt
            newrow[9] = custdt.Rows.Count > 0 ? Convert.ToString(custdt.Rows[0][5]) : "";             //开始合作时间 --来源:custdt
            newrow[10] = noticedt.Rows.Count > 0 ? Convert.ToString(noticedt.Rows[0][1]) : "";        //K3出库单号 --来源:noticedt *
            newrow[11] = noticedt.Rows.Count > 0 ? Convert.ToString(noticedt.Rows[0][2]) : "";        //销售订单号  --来源:noticedt *
            newrow[12] = custdt.Rows.Count > 0 ? Convert.ToString(custdt.Rows[0][6]) : "";            //经营区域 --来源:custdt
            newrow[13] = custdt.Rows.Count > 0 ? Convert.ToInt32(custdt.Rows[0][7]) : 0;              //币别     --来源:custdt
            newrow[14] = receiveable;                                                                 //月均销售额(元)  --来源:receiveable
            newrow[15] = noticedt.Rows.Count > 0 ? Convert.ToDecimal(noticedt.Rows[0][4]) : 0;        //信用周期(天)    --来源:noticedt *
            newrow[16] = custdt.Rows.Count > 0 ? Convert.ToString(custdt.Rows[0][8]) : "";            //收款条件  --来源:custdt
            newrow[17] = amountdt.Rows.Count > 0 ? Convert.ToDecimal(amountdt.Rows[0][1]) : 0;        //超额欠款(元)    --来源:amountdt * change date:20221105
            newrow[18] = noticedt.Rows.Count > 0 ? Convert.ToString(noticedt.Rows[0][6]) : "";        //超期天数(天)    --来源:noticedt *

            newrow[19] = receivebillDt.Rows.Count > 0 ? Convert.ToDecimal(receivebillDt.Rows[0][1]) : 0;            //最后一次收款金额  --来源:receivebillDt
            newrow[20] = receivebillDt.Rows.Count > 0 ? Convert.ToString(receivebillDt.Rows[0][0]) : "";            //最后一次收款时间  --来源:receivebillDt

            newrow[21] = noticedt.Rows.Count > 0 ? Convert.ToDecimal(noticedt.Rows[0][7]) : 0;         //当天申请出货金额(元) --来源:noticedt *

            // newrow[22] = noticedt.Rows.Count > 0 ? Convert.ToDecimal(noticedt.Rows[0][8]) : 0;         //出货后超出信用额度欠款(元) --来源:noticedt *

            if (amountdt.Rows.Count == 0 && noticedt.Rows.Count == 0)
            {
                amount = 0;
            }
            else if (amountdt.Rows.Count == 0 && noticedt.Rows.Count>0)
            {
                amount = Convert.ToDecimal(noticedt.Rows[0][7]);
            }
            else if (noticedt.Rows.Count == 0 && amountdt.Rows.Count>0)
            {
                amount = Convert.ToDecimal(amountdt.Rows[0][1]);
            }
            else
            {
                amount = Convert.ToDecimal(amountdt.Rows[0][1]) + Convert.ToDecimal(noticedt.Rows[0][7]);
            }

            newrow[22] = amount;
            //出货后超出信用额度欠款(元) 来源:amountdt change date:20221105  出货后超出信用额度欠款(元)=超额欠款(元)+当天申请出货金额(元)
            newrow[25] = amountdt.Rows.Count > 0 ? Convert.ToDecimal(amountdt.Rows[0][0]) : 0;         //期末余额 --来源:amountdt change date:20221105

            dt.Rows.Add(newrow);
            return dt;
        }

        /// <summary>
        /// 根据获取的临时表记录,并利用OA API创建流程接口,创建流程
        /// </summary>
        /// <param name="createid">用户ID;创建流程时必需</param>
        /// <param name="updatelist"></param>
        /// <returns></returns>
        private string CreateOaWorkFlow(int createid,string updatelist)
        {
            var result = string.Empty;

            try
            {
                WorkflowService workflow = new WorkflowService();

                WorkflowRequestInfo workflowRequestInfo = new WorkflowRequestInfo();
                WorkflowBaseInfo baseInfo = new WorkflowBaseInfo();

                //设置工作流ID_必须添加(重)
                baseInfo.workflowId = "68";  //"129";
                baseInfo.workflowName = "超额客户出货";

                //设置如能否修改 查询等基础信息
                workflowRequestInfo.canView = true;
                workflowRequestInfo.canEdit = true;
                workflowRequestInfo.requestName = baseInfo.workflowName;   //设置标题_此项必须添加(重)
                workflowRequestInfo.requestLevel = "0";
                workflowRequestInfo.creatorId = Convert.ToString(createid);  //设置创建者ID(重要:创建流程时必须填)

                workflowRequestInfo.workflowBaseInfo = baseInfo;

                //主表设置
                WorkflowMainTableInfo workflowMainTableInfo = new WorkflowMainTableInfo();
                WorkflowRequestTableRecord[] workflowRequestTableRecords = new WorkflowRequestTableRecord[1]; //设置主表字段有一条记录
                WorkflowRequestTableField[] workflowtabFields = new WorkflowRequestTableField[1];  //设置主表有多少个字段


                #region 循环设置各列字段的相关信息-注:因作为K3插件时不能使用以下代码,导致不能在插入成功并获取requestid后,将明细记录进行插入;故不使用(20220904)
                //循环设置各列字段的相关信息
                //workflowtabFields[0] = new WorkflowRequestTableField();
                //workflowtabFields[0].fieldName = "sqr";
                //workflowtabFields[0].fieldValue = "249";
                //workflowtabFields[0].view = true;
                //workflowtabFields[0].edit = true;

                //for (var i = 0; i < resultdt.Columns.Count; i++)
                //{
                //    workflowtabFields[i] = new WorkflowRequestTableField();
                //    workflowtabFields[i].fieldName = resultdt.Columns[i].ColumnName;  //字段名称
                //    workflowtabFields[i].fieldValue = Convert.ToString(resultdt.Rows[0][i]); //字段值
                //    workflowtabFields[i].view = true;  //能否查阅
                //    //除‘销售订单号’(11)可以修改外,其它都不能修改
                //    //workflowtabFields[i].edit = i == 11;
                //    workflowtabFields[i].edit = true;   //这里必须要设置为true,可修改,不然会插入不到记录至表格
                //}
                #endregion

                //将workflowtableFields所设置的字段加载到workflowRequestTableRecords内
                workflowRequestTableRecords[0] = new WorkflowRequestTableRecord();
                workflowRequestTableRecords[0].workflowRequestTableFields = workflowtabFields;

                //然后将workflowRequestTableRecords加载到workflowMainTableInfo.requestRecords内
                workflowMainTableInfo.requestRecords = workflowRequestTableRecords;

                //最后将workflowMainTableInfo加载到workflowRequestInfo.workflowMainTableInfo内
                workflowRequestInfo.workflowMainTableInfo = workflowMainTableInfo;

                //执行doCreateWorkflowRequest()方法,若返回值>0 就成功;反之,出现异常
                var requestid = workflow.doCreateWorkflowRequest(workflowRequestInfo, createid);

                //在获取requestid后,对相关值进行更新
                if (Convert.ToInt32(requestid) > 0)
                {
                    searchDt.UpdateRecord(requestid, updatelist);
                    result = "Finish";
                }
                else
                {
                    result = "error";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

    }
}

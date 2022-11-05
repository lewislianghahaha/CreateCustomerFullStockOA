using Kingdee.BOS.Orm.Exceptions;

namespace CreateCustomerFullStockOA
{
    //SQL语句
    public class SqlList
    {
        private string _result;

        /// <summary>
        /// 发货通知单相关信息获取
        /// </summary>
        /// <returns></returns>
        public string SearchDeliveryNotice(string orderno)
        {
            _result =
                $@"
                    SELECT X.FCUSTOMERID,
			        X.K3出库单号,X.销售订单号,
                    X.[当前信用额度(元)],X.[信用周期(天)]
			        ,X.[超额欠款(元)] --(NO NEED)
					,X.[超期天数(天)]
			        ,X.当天申请出货金额		
                    ,X.[超额欠款(元)]+X.当天申请出货金额 [出货后超出信用额度欠款(元)]   --公式：超额欠款+当天申请出货金额 (NO NEED)
					,X.单据日期
					,X.客户编码

                    FROM (
                    SELECT A.FCUSTOMERID,A.FBILLNO K3出库单号,A.F_YTC_TEXT4 销售订单号,
                                ROUND(A.F_YTC_DECIMAL,2) [当前信用额度(元)]
			                    ,A.F_YTC_INTEGER [信用周期(天)]
			                    ,ROUND(A.F_YTC_DECIMAL2,2) [超额欠款(元)]
			                    ,A.F_YTC_INTEGER1 [超期天数(天)]
			                    ,ISNULL(SUM(B.FALLAMOUNT_LC),0) 当天申请出货金额
								,CONVERT(VARCHAR(10),A.FDATE,23) 单据日期
								,C.FNUMBER 客户编码

                    FROM dbo.T_SAL_DELIVERYNOTICE A
                    INNER JOIN dbo.T_SAL_DELIVERYNOTICEENTRY_F B ON A.FID=B.FID
					INNER JOIN dbo.T_BD_CUSTOMER C ON A.FCUSTOMERID=C.FCUSTID
                    WHERE /*a.FDOCUMENTSTATUS='C'    --需为已审核 
                    AND*/ A.FBILLNO='{orderno}'      --'FHTZD160918'  
                    GROUP BY A.FCUSTOMERID,A.FBILLNO,A.F_YTC_TEXT4,A.F_YTC_DECIMAL
			                    ,A.F_YTC_INTEGER,A.F_YTC_DECIMAL2,A.F_YTC_INTEGER1
								,A.FDATE,C.FNUMBER)X
                  ";

            return _result;
        }

        /// <summary>
        /// 根据用户名称获取OA-用户ID及部门ID信息
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string SearchOaInfo(string username)
        {
            _result = $@"
                            SELECT A.ID 用户ID,A.lastname 名称,B.id 部门ID,C.ID 岗位ID,CONVERT(VARCHAR(10),GETDATE(),23) 申请日期
                            --,B.departmentmark 部门 
                            FROM dbo.HrmResource A
                            INNER JOIN dbo.HrmDepartment B ON A.departmentid=B.id
                            INNER JOIN HrmJobTitles C ON A.jobtitle=C.ID
                            WHERE A.lastname='{username}' --'梁嘉杰'--ID='249'
                        ";
            return _result;
        }

        /// <summary>
        /// 根据客户ID查询相关信息
        /// </summary>
        /// <param name="custid"></param>
        /// <returns></returns>
        public string SearchCustomerInfo(int custid)
        {
            _result = $@"
                            SELECT a.FCUSTID,A.FNUMBER 客户代码,B.FNAME 客户名称,A.F_YTC_TEXT3 [经销商名称(营业执照为准)]
                                    ,A.F_YTC_TEXT2 法人姓名,CONVERT(VARCHAR(10),A.F_YTC_DATE,23) 开始合作时间
                                    ,A.F_YTC_TEXT13 经营区域
		                            ,CASE A.FTRADINGCURRID WHEN 1 THEN  0  WHEN 7 THEN 1 ELSE 2 END 币别  --0:人民币 1:美元 2:卢布
		                            ,C.FNAME  收款条件
                            FROM dbo.T_BD_CUSTOMER A
                            INNER JOIN dbo.T_BD_CUSTOMER_L B ON A.FCUSTID=B.FCUSTID
                            INNER JOIN T_BD_RECCONDITION_L C ON A.FRECCONDITIONID=C.FID AND C.FLOCALEID=2052
                            WHERE A.FCUSTID='{custid}' --FNUMBER='086.02.757.002'   --客户编码
                        ";

            return _result;
        }

        /// <summary>
        /// 根据客户ID获取收款单相关信息
        /// </summary>
        /// <param name="custid"></param>
        /// <returns></returns>
        public string SearchReciveBillInfo(int custid)
        {
            _result = $@"
                            SELECT A.FID,a.FDATE,A.FREALRECAMOUNTFOR 实收金额
                            INTO #TEMP0
                            FROM dbo.T_AR_RECEIVEBILL A
                            WHERE A.FCONTACTUNIT='{custid}'--'137411'    --以客户ID为条件
                            ORDER BY A.FDATE DESC

                            SELECT TOP 1 CONVERT(VARCHAR(10),A.FDATE,23) 最后一次收款时间,ROUND(A.实收金额,2) 最后一次收款金额
                            FROM #TEMP0 A
                            ORDER BY A.FID DESC
                        ";

            return _result;
        }

        /// <summary>
        /// 根据客户ID获取应收单相关信息
        /// </summary>
        /// <param name="custid"></param>
        /// <returns></returns>
        public string SearchReceivableInfo(int custid)
        {
            _result =
                $@"
                           DECLARE
	                            @SDT VARCHAR(100),
	                            @EDT VARCHAR(100),
	                            @MON INT;
                            BEGIN

	                            SET @SDT=CONVERT(VARCHAR(10),YEAR(GETDATE()))+'-'+'01'+'-'+'01'    --当前年份起始日期
	                            SET @EDT=CONVERT(VARCHAR(10),GETDATE(),23)                         --当天日期
	                            SET @MON=MONTH(GETDATE())                                          --当前月份

	                            SELECT  ROUND(ISNULL(SUM(B.FALLAMOUNT)/@MON,0),2) [月均销售额(元)]
	                            FROM dbo.T_AR_RECEIVABLE A
	                            INNER JOIN dbo.T_AR_RECEIVABLEFIN B ON A.FID=B.FID
	                            WHERE A.FCUSTOMERID='{custid}'--'137411'   --086.02.757.002
	                            AND CONVERT(VARCHAR(100),A.FDATE,23)>=@SDT
	                            AND CONVERT(VARCHAR(100),A.FDATE,23)<=@EDT
                            END 
                        ";
            return _result;
        }

        /// <summary>
        /// 检查客户是否有信用额度,有才将客户的所有相关信息插入(no need)
        /// </summary>
        /// <param name="custid"></param>
        /// <returns></returns>
        public string CheckCustAccount(int custid)
        {
            _result = $@"
                            SELECT X.ID,X.FCUSTID
                            FROM (
                                SELECT  ROW_NUMBER() OVER (ORDER BY T1.FNUMBER) id,T1.FCUSTID
				                FROM  dbo.T_BD_CUSTOMER T1 
                                LEFT JOIN dbo.T_CRE_CUSTARCHIVESENTRY T4 ON T1.FCUSTID = T4.FOBJECTID 
				                LEFT JOIN (
				                                    SELECT   X.FCUSTOMERID, SUM(X1.FALLAMOUNT) - SUM(X1.FRECEIVEAMOUNT) YQAmount
								                    FROM      dbo.T_AR_RECEIVABLE X 
								                    INNER JOIN  dbo.T_AR_RECEIVABLEENTRY X1 ON X.FID = X1.FID 
								                    LEFT JOIN  dbo.T_CRE_CUSTARCHIVESENTRY X2 ON X.FCUSTOMERID = X2.FOBJECTID
								                    WHERE   X.FWRITTENOFFSTATUS IN ('A', 'B') 
								                -- AND CONVERT(datetime, /*@fstrdate*/ GETDATE()) > X.FENDDATE + X2.FOVERDAYS change date:20220902
								                    GROUP BY X.FCUSTOMERID
								                ) AS T5 ON T1.FCUSTID = T5.FCUSTOMERID 
                                WHERE   T1.FDOCUMENTSTATUS = 'C' AND ISNULL(T5.YQAmount, 0) > 0
                                )X
                            WHERE X.FCUSTID='{custid}'
                        ";
            return _result;
        }

        /// <summary>
        /// 根据发货通知单-客户-获取FISCREDITCHECK (启用信息管理) 0：否 1：是
        /// </summary>
        /// <param name="orderno">单据编号</param>
        /// <returns></returns>
        public string CheckisOpen(string orderno)
        {
            _result = $@"
                           SELECT A.FCUSTOMERID,B.FISCREDITCHECK
                           FROM dbo.T_SAL_DELIVERYNOTICE A
                           INNER JOIN dbo.T_BD_CUSTOMER B ON A.FCUSTOMERID=B.FCUSTID
                           WHERE A.FBILLNO='{orderno}'";

            return _result;
        }


        /// <summary>
        /// 根据指定条件获取‘各事业部风险账报表’记录
        /// </summary>
        /// <param name="sdt">开始日期</param>
        /// <param name="edt">结束日期</param>
        /// <param name="scustcode">开始客户编码</param>
        /// <param name="ecustcode">结束客户编码</param>
        /// <returns></returns>
        public string SearchAmount(string sdt, string edt, string scustcode, string ecustcode)
        {
            _result = $@"BEGIN

	                            CREATE TABLE #TEMPDIY(
								                            往来单位编码 NVARCHAR(250),
								                            往来单位名称 NVARCHAR(300),
								                            大区 NVARCHAR(100),
								                            销售部门 NVARCHAR(200),
								                            销售员 NVARCHAR(100),
								                            币别 NVARCHAR(20),
								                            期初余额 DECIMAL(25,5),
								                            本期应收 DECIMAL(25,5),
								                            本期收款 DECIMAL(25,5),
								                            本期冲销额 DECIMAL(25,5),
								                            期末余额 DECIMAL(25,5),
								                            信用额周期 NVARCHAR(100),
								                            信用额度 DECIMAL(25,5),
								                            客户开票名称 NVARCHAR(250)
					                                 )

                            ------将‘应收款汇总表_DIY’按照条件将相关记录插入至#TEMPDIY临时表内
	                            INSERT INTO #TEMPDIY EXEC 应收款汇总表_DIY '{sdt}','{edt}','{scustcode}','{ecustcode}','','','',''

		                            SELECT A.期末余额
		                                   ,ISNULL(A.期末余额,0)-ISNULL(A.信用额度,0) 超额     --公式:期末余额-信用额度
		                            FROM #TEMPDIY A

		                            --DROP TABLE #TEMPDIY

                            END";

            return _result;
        }

        /// <summary>
        /// 插入成功后,更新OA相关信息
        /// </summary>
        /// <param name="requestid"></param>
        /// <param name="updatelistvalue"></param>
        /// <returns></returns>
        public string UpdateRecord(string requestid, string updatelistvalue)
        {
            _result =
                $@"
                    update formtable_main_74 set {updatelistvalue} where requestid='{requestid}'
                  ";

            return _result;
        }

    }
}

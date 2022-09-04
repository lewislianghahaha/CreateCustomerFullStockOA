using System;
using System.Data;

namespace CreateCustomerFullStockOA
{
    //临时表
    public class Tempdt
    {
        /// <summary>
        /// 插入OA新增流程所需字段,最后作为新增流程接口时使用
        /// </summary>
        /// <returns></returns>
        public DataTable InsertOaRecord()
        {
            var dt = new DataTable();
            for (var i = 0; i < 23; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    //申请人
                    case 0:
                        dc.ColumnName = "sqr";
                        dc.DataType = Type.GetType("System.Int32");
                        break;
                    //申请日期
                    case 1:
                        dc.ColumnName = "sqrq";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //申请部门
                    case 2:
                        dc.ColumnName = "sqbm";
                        dc.DataType = Type.GetType("System.Int32");
                        break;
                    //岗位
                    case 3:
                        dc.ColumnName = "jobtitle";
                        dc.DataType = Type.GetType("System.Int32");
                        break;
                    //客户代码
                    case 4:
                        dc.ColumnName = "khdm";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //客户名称
                    case 5:
                        dc.ColumnName = "khmc";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //当前信用额度(元)
                    case 6:
                        dc.ColumnName = "dqxyedy";
                        dc.DataType = Type.GetType("System.Decimal"); 
                        break;
                    //经销商名称(营业执照为准)
                    case 7:
                        dc.ColumnName = "jxsmcyyzzwz";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //法人姓名
                    case 8:
                        dc.ColumnName = "frxm";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //开始合作时间
                    case 9:
                        dc.ColumnName = "kshzsj";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //K3出库单号
                    case 10:
                        dc.ColumnName = "k3ckdh";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //销售订单号
                    case 11:
                        dc.ColumnName = "xsddh";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //经营区域
                    case 12:
                        dc.ColumnName = "jyqy";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //币别
                    case 13:
                        dc.ColumnName = "bibie";
                        dc.DataType = Type.GetType("System.Int32"); 
                        break;
                    //月均销售额(元)
                    case 14:
                        dc.ColumnName = "yjxsey";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    //信用周期(天)
                    case 15:
                        dc.ColumnName = "xyzqt";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    //收款条件
                    case 16:
                        dc.ColumnName = "sktj";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //超额欠款(元)
                    case 17:
                        dc.ColumnName = "ceqky";
                        dc.DataType = Type.GetType("System.Decimal"); 
                        break;
                    //超期天数(天)
                    case 18:
                        dc.ColumnName = "cqtst";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //最后一次收款金额
                    case 19:
                        dc.ColumnName = "zhycskje";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    //最后一次收款时间
                    case 20:
                        dc.ColumnName = "zhycsksj";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //当天申请出货金额(元)
                    case 21:
                        dc.ColumnName = "dtsqchjey";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    //出货后超出信用额度欠款(元)
                    case 22:
                        dc.ColumnName = "chhccxyedqky";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }
    }
}

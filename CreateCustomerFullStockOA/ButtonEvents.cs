using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace CreateCustomerFullStockOA
{
    public class ButtonEvents : AbstractBillPlugIn
    {
        GenerateDt generateDt=new GenerateDt();

        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);

            if (e.BarItemKey == "tbCreateCustFullStock")
            {
                var docScddIds1 = View.Model.DataObject;
                //获取表头中单据编号信息(注:这里的BillNo为单据编号中"绑定实体属性"项中获得)
                var orderno = docScddIds1["BillNo"].ToString();
                var orderstatus = docScddIds1["DocumentStatus"].ToString();
                //需检测此单据为‘审核’状态才能继续
                if (orderstatus == "C")
                {
                    //获取当前登录用户
                    var username = this.Context.UserName;
                    //执行运算并返回相关结果
                    var mesage = generateDt.GetMessageIntoOa(orderno, username);
                    //View.ShowMessage(mesage);
                    View.ShowMessage(mesage != "Finish" ? $"新增超额出货异常,原因:'{mesage}'" : "新增成功,请打开OA,并留意右下角的OA信息提示");
                }
                else
                {
                    View.ShowErrMessage($"单据'{orderno}'不为审核状态,不能继续");
                }
            }

        }
    }
}

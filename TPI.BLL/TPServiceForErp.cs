using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPI.Common;
using TPI.Common.Busi;

namespace TPI.BLL
{
    /// <summary>
    /// 对erp实现实现的接口(暂不提供)
    /// </summary>
    public class TPServiceForErp : ITPServiceForErp
    {


        /// <summary>
        /// 从实际的平台中获取相应代理。
        /// </summary>
        /// <returns></returns>
        public List<Order> GetOrder(string branchID, string TPId, string billID)
        {
            //根据传过来的分公司、平台、确定平台获取对象
            MainApplication ma = MainApplication.GetInstance();
            ITPProxy proxy = ma.EnvironmentConfig.CreateTPProxy(branchID,TPId);

            if (proxy == null)
            { }


            //向 外部平台获取信息，并返回
            List<Order> order = proxy.GetOrder1();

            return order;

        }

        public DataTable GetOrderInfos(string branchID, string TPId)
        {
            throw new NotImplementedException();
        }

        public bool WriteBackOrderState(string branchID, string TPId, string billID)
        {
            throw new NotImplementedException();
        }
    }

    public class proErp
    {
        public void InsertProduct(string branchID, string TPId)
        {
            new pro().InsertProduct(branchID, TPId);
        }

    }

    public class schedulePro
    {
        public string branch { get; set; }
        public string tpid { get; set; }

        private pro p = new pro();

        public void Exe()
        {
            p.InsertProduct(branch, tpid);
        }
    }

    public class pro
    {
        public void InsertProduct(string branchID, string TPId)
        {
            //根据传过来的分公司、平台、确定平台获取对象
            //MainApplication ma = MainApplication.GetInstance();
            //ITPProxy proxy = ma.EnvironmentConfig.CreateTPProxy(branchID, TPId);

            //if (proxy == null)
            //{
            //    return;
            //}

            ////1 从远程取回商品资料
            //List <Goods> goods = proxy.GetProductInfo( branchID,  TPId);
            //if (goods == null)
            //{
            //    return;
            //}
            ////2 调用ERP接口将商品资料写入ERP
            //ErpProxy erp = new ErpProxy();
            //erp.InsertProductInfo(goods);
        }
    }
}

using EntitysLayer.Entitys;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TPI.Common;
using TPI.Common.BJConfig;
using TPI.Common.Busi;
using TPI.Common.FJConfig;
using TPI.Common.HNConfig;
using TPI.Common.SHConfig;

namespace TPI.BLL.ActiveMode
{
    /// <summary>
    /// 主动服务模式类，它作为erp tp 之间的联系类，负责调度两边的方法
    /// 来完成两个系统间的数据交互。
    /// </summary>
    public class AcitveModeService
    {
        private ITPProxy tpProxy;
        private ITPProxyCC tpProxycc;
        private IErpProxy erpProxy;
        private string branchid;
        private string platid;
        private string erpurl; 
        private string customerpurl; 
        private string tpurl;

        private ILog log;

        /// <summary>
        /// 设置日志
        /// </summary>
        /// <param name="logger"></param>
        public void SetLog(ILog logger)
        {
            log = logger;
            (erpProxy as ILogManager).SetLog(logger);
            (tpProxy as ILogManager1).SetLog(logger);
            if (branchid == "FWP")
            {
                (tpProxycc as ILogManager1).SetLog(logger);
            }
        }

        /// <summary>
        /// 设置ERP的API地址
        /// </summary>
        public void SetERPUrl()
        {
            (erpProxy as ILogManager).SetERPUrl(erpurl);
        }

        /// <summary>
        /// 设置ERP的自定义API地址
        /// </summary>
        public void SetCustomERPUrl()
        {
            (erpProxy as ILogManager).SetCustomERPUrl(customerpurl);
        }

        /// <summary>
        /// 设置平台API地址
        /// </summary>
        public void SetTPUrl()
        {
            (tpProxy as ILogManager1).SetTPUrl(tpurl);
            if (branchid == "FWP")
            {
                (tpProxycc as ILogManager1).SetTPUrl(tpurl);
            }
        }

        public AcitveModeService(string branchID, string TPID)
        {
            //根据传过来的分公司、平台、确定平台获取对象
            MainApplication ma = MainApplication.GetInstance();
            tpProxy = ma.EnvironmentConfig.CreateTPProxy(branchID, TPID);
            if (branchID == "FWP")
            {
                tpProxycc = ma.EnvironmentConfig.CreateTPProxyCC(branchID, TPID);
            }
            erpurl = ma.EnvironmentConfig.GetERPURL(branchID);
            customerpurl = ma.EnvironmentConfig.GetCustomERPURL(branchID);
            tpurl = ma.EnvironmentConfig.GetTPURL(branchID, TPID);
            //实例化erp实现的对象
            erpProxy = new ErpProxy();
            branchid = branchID;
            platid = TPID;
        }

        #region 杭州

        #region 测试框架用

        //杭州test
        public void HZTEST()
        {
            tpProxy.Test(branchid, platid);
            erpProxy.Test(branchid, platid);
        }
        #endregion

        #region 基础资料
        //商品资料
        public void HZGOOD()
        {
            int iswc = 0;
            DateTime date;
            int currentpage = 1;
            //Console.WriteLine("service1 executing ......");
            //log.Info("商品资料开始......");
            #region test
            //List<Goods> goods = tpProxy.GetProductInfo(branchid, tpid, "", currentpage, out iswc);

            ////2 调用ERP接口将商品资料写入ERP
            //if (erpProxy.InsertProductInfo(goods))
            //{
            //    log.Info("月份:" + 1 + " 页码:" + currentpage + " 商品资料执行成功");
            //}
            //else
            //{
            //    log.Info("月份:" + 1 + " 页码:" + currentpage + "商品资料执行失败");
            //}
            #endregion
            #region zhengshi

            if (tpProxy == null)
            {
                return;
            }
            //从ERP获取当前应该获取的月份和页码数
            //获取月份
            string date1 = erpProxy.IsExistProductInfo(branchid, platid);
            if (date1 == "")
            {
                date = DateTime.Parse("2016-05-01");
            }
            else
                date = DateTime.Parse(date1);
            if (date.Year < 2016)
                date = DateTime.Parse("2016-05-01");
            int month = DateTime.Now.Month + (DateTime.Now.Year - date.Year) * 12 - date.Month;
            for (int a = 0; a <= month; a++)
            {
                iswc = 0;
                while (iswc == 0)
                {
                    string d = "";
                    if ((date.Month + a) > 12)
                    {
                        d = date.Year + 1 + "-" + (date.Month + a) % 12 + "-" + "01";
                    }
                    else
                        //获取页码数
                        d = date.Year + "-" + (date.Month + a) + "-" + "01";
                    int count = erpProxy.GetProductInfoCount(branchid, platid, d);
                    if (count <= 0)
                    {
                        currentpage = 1;
                    }
                    else
                    {
                        currentpage = count / 20 + 1;
                    }
                    string mon = "";
                    if (a + date.Month < 10)
                    {
                        mon = date.Year + "-0" + (a + date.Month);
                    }
                    else
                    {
                        if ((date.Month + a) > 12)
                        {
                            if ((a + date.Month) % 12 < 10)
                            {
                                mon = date.Year + 1 + "-0" + (a + date.Month) % 12;
                            }
                            else
                                mon = date.Year + 1 + "-" + (date.Month + a) % 12;
                        }
                        else
                            mon = date.Year + "-" + (a + date.Month);
                    }
                    //1 从远程取回商品资料
                    List<Goods> goods = tpProxy.GetProductInfo(branchid, platid, mon, currentpage, out iswc);
                    //List<Goods> goods = tpProxy.GetProductInfo(branchid, tpid, "2016-05", 550, out iswc);
                    if (goods == null)
                    {
                        continue;
                    }
                    if (goods.Count == 0)
                    {
                        continue;
                    }
                    //2 调用ERP接口将商品资料写入ERP
                    if (erpProxy.InsertProductInfo(goods))
                    {
                        log.Info("月份:" + mon + " 页码:" + currentpage + " 药品信息执行成功");
                    }
                    else
                    {
                        log.Info("月份:" + mon + " 页码:" + currentpage + "药品信息执行失败");
                    }
                }
            }
            #endregion
        }

        //商品资料
        public void HZGOODEXIST()
        {
            #region zhengshi

            if (tpProxy == null)
            {
                return;
            }

            //1 从远程取回商品资料
            //int a = tpProxy.GetProductInfoByID(branchid, tpid, "10000");
            tpProxy.GetProductInfoByID(branchid, platid, "-1,1");

            #endregion
        }

        //医院信息
        public void HZHOSPITAL()
        {
            int iswc = 0;
            DateTime date;
            int currentpage = 1;
            //Console.WriteLine("service2 executing ......");
            //log.Info("医院信息开始......");
            #region test
            //List<Hospital> hos = tpProxy.GetHospitalInfo(branchid, tpid, "", currentpage, out iswc);
            ////2 调用ERP接口将医院资料写入ERP
            //if (erpProxy.InsertHospitalInfo(hos))
            //{
            //    log.Info("医院资料执行成功");
            //}
            //else
            //{
            //    log.Info("医院资料执行失败");
            //}
            #endregion
            #region zhengshi

            if (tpProxy == null)
            {
                return;
            }
            //从ERP获取当前应该获取的月份和页码数
            //获取月份
            string date1 = erpProxy.IsExistHospitalPSDInfo(branchid, platid);
            if (date1 == "")
            {
                date = DateTime.Parse("2016-05-01");
            }
            else
                date = DateTime.Parse(date1);
            if (date.Year < 2016)
                date = DateTime.Parse("2016-05-01");
            //int month = DateTime.Now.Month - date.Month;
            int month = DateTime.Now.Month + (DateTime.Now.Year - date.Year) * 12 - date.Month;
            for (int a = 0; a <= month; a++)
            {
                iswc = 0;
                while (iswc == 0)
                {
                    //获取页码数
                    //string d = date.Year + "-" + (date.Month + a) + "-" + "01";
                    string d = "";
                    if ((date.Month + a) > 12)
                    {
                        d = date.Year + 1 + "-" + (date.Month + a) % 12 + "-" + "01";
                    }
                    else
                        //获取页码数
                        d = date.Year + "-" + (date.Month + a) + "-" + "01";
                    int count = erpProxy.GetHospitalPSDInfoCount(branchid, platid, d);
                    if (count <= 0)
                    {
                        currentpage = 1;
                    }
                    else
                    {
                        currentpage = count / 20 + 1;
                    }
                    string mon = "";
                    if (a + date.Month < 10)
                    {
                        mon = date.Year + "-0" + (a + date.Month);
                    }
                    else
                    {
                        if ((date.Month + a) > 12)
                        {
                            if ((a + date.Month) % 12 < 10)
                            {
                                mon = date.Year + 1 + "-0" + (a + date.Month) % 12;
                            }
                            else
                                mon = date.Year + 1 + "-" + (date.Month + a) % 12;
                        }
                        else
                            mon = date.Year + "-" + (a + date.Month);
                    }
                    //从远程取回医院资料
                    List<HospitalPSD> hos = tpProxy.GetHospitalPSDInfo(branchid, platid, mon, currentpage, out iswc);
                    //List<HospitalPSD> hos = tpProxy.GetHospitalPSDInfo(branchid, tpid, "2016-05", 1, out iswc);
                    if (hos == null)
                    {
                        continue;
                    }
                    if (hos.Count == 0)
                    {
                        continue;
                    }
                    //2 调用ERP接口将医院资料写入ERP
                    if (erpProxy.InsertHospitalPSDInfo(hos))
                    {
                        log.Info("月份:" + mon + " 页码:" + currentpage + " 医院资料执行成功");
                    }
                    else
                    {
                        log.Info("月份:" + mon + " 页码:" + currentpage + "医院资料执行失败");
                    }
                }
            }
            #endregion
        }
        //医院信息
        public void HZHOSPITALEXIST()
        {

            #region zhengshi

            if (tpProxy == null)
            {
                return;
            }

            //从远程取回医院资料
            //int a = tpProxy.GetHospitalInfoByID(branchid, tpid, "1234");
            tpProxy.GetHospitalInfoByID(branchid, platid, "11111,000B7137-7D9E-465C-89C1-AFD5485EA213");

            #endregion
        }

        //厂家信息
        public void HZCOMPANY()
        {
            int iswc = 0;
            DateTime date;
            int currentpage = 1;
            //Console.WriteLine("service2 executing ......");
            //log.Info("厂家信息开始......");
            #region test
            //List<Companys> comp = tpProxy.GetCompanyInfo(branchid, tpid, "", currentpage, out iswc);
            ////2 调用ERP接口将厂家资料写入ERP
            //if (erpProxy.InsertCompanyInfo(comp))
            //{
            //    log.Info("厂家资料执行成功");
            //}
            //else
            //{
            //    log.Info("厂家资料执行失败");
            //}
            #endregion
            #region zhengshi
            if (tpProxy == null)
            {
                return;
            }
            //获取月份
            string date1 = erpProxy.IsExistCompanyInfo(branchid, platid);
            if (date1 == "")
            {
                date = DateTime.Parse("2016-05-01");
            }
            else
                date = DateTime.Parse(date1);
            if (date.Year < 2016)
                date = DateTime.Parse("2016-05-01");
            //int month = DateTime.Now.Month - date.Month;
            int month = DateTime.Now.Month + (DateTime.Now.Year - date.Year) * 12 - date.Month;
            for (int a = 0; a <= month; a++)
            {
                iswc = 0;
                while (iswc == 0)
                {
                    //获取页码数
                    //string d = date.Year + "-" + (date.Month + a) + "-" + "01";
                    string d = "";
                    if ((date.Month + a) > 12)
                    {
                        d = date.Year + 1 + "-" + (date.Month + a) % 12 + "-" + "01";
                    }
                    else
                        //获取页码数
                        d = date.Year + "-" + (date.Month + a) + "-" + "01";
                    int count = erpProxy.GetCompanyInfoCount(branchid, platid, d);
                    if (count <= 0)
                    {
                        currentpage = 1;
                    }
                    else
                    {
                        currentpage = count / 20 + 1;
                    }
                    string mon = "";
                    if (a + date.Month < 10)
                    {
                        mon = date.Year + "-0" + (a + date.Month);
                    }
                    else
                    {
                        if ((date.Month + a) > 12)
                        {
                            if ((a + date.Month) % 12 < 10)
                            {
                                mon = date.Year + 1 + "-0" + (a + date.Month) % 12;
                            }
                            else
                                mon = date.Year + 1 + "-" + (date.Month + a) % 12;
                        }
                        else
                            mon = date.Year + "-" + (a + date.Month);
                    }
                    //从远程取回厂家资料
                    List<Companys> comp = tpProxy.GetCompanyInfo(branchid, platid, mon, currentpage, out iswc);
                    //List<Companys> comp = tpProxy.GetCompanyInfo(branchid, tpid, "2016-05", 1, out iswc);
                    if (comp == null)
                    {
                        continue;
                    }
                    if (comp.Count == 0)
                    {
                        continue;
                    }
                    //2 调用ERP接口将厂家资料写入ERP
                    if (erpProxy.InsertCompanyInfo(comp))
                    {
                        log.Info("月份:" + mon + " 页码:" + currentpage + " 厂家资料执行成功");
                    }
                    else
                    {
                        log.Info("月份:" + mon + " 页码:" + currentpage + "厂家资料执行失败");
                    }
                }
            }
            #endregion

        }

        #endregion

        #region 订单

        //订单下载
        public void HZORDER()
        {
            int iswc = 0;
            DateTime date;

            log.Info("订单下载开始......");
            if (tpProxy == null)
            {
                return;
            }
            //从ERP获取当前最大的提交时间
            string a = erpProxy.GetLastDDTJRQ(branchid, platid);
            if (a == null)
            {
                date = DateTime.Parse("2016-05-01 00:00:00");
            }
            else
            {
                if (a.Replace("\"", "") != null && a.Replace("\"", "") != "")
                {
                    date = DateTime.Parse(a.Replace("\"", ""));
                }
                else
                {
                    date = DateTime.Parse("2016-08-01 00:00:00");
                }
            }
            //date = DateTime.Parse("2016-08-01 00:00:00");
            int currentpage = 1;
            DateTime enddate = DateTime.Now;
            while (iswc == 0)
            {
                //从远程取回订单
                List<Order> comp = tpProxy.GetOrder(branchid, platid, date, enddate, out iswc, currentpage);
                if (comp == null)
                {
                    return;
                }
                if (comp.Count == 0)
                {
                    return;
                }
                //2 调用ERP接口将厂家资料写入ERP
                if (erpProxy.UploadOrder(comp))
                {
                    log.Info("订单下载执行成功");
                }
                else
                {
                    log.Info("订单下载执行失败");
                    return;
                }
                currentpage += 10;
            }
        }

        //订单阅读
        public void HZORDERREAD()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("订单阅读开始......");
            //1 调用ERP接口获取订单阅读信息 ddread=0
            List<Order> bill = erpProxy.GetReadOrderInfo(branchid, platid, 5);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<Order> bill1 = tpProxy.ReadBill(branchid, platid, bill);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateOrder(bill1))
            {
                log.Info("订单阅读回写执行成功");
            }
            else
            {
                log.Info("订单阅读回写执行失败");
            }
        }

        //订单响应
        public void HZORDERRESPONSE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("订单响应开始......");
            #region 正常响应
            //1 调用ERP接口获取订单响应信息
            List<Order> bill = erpProxy.GetResponseOrderInfo(branchid, platid, 5, 2);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<Order> bill1 = tpProxy.ResponseBill(branchid, platid, bill, 0);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateOrder(bill1))
            {
                log.Info("订单响应回写执行成功");
            }
            else
            {
                log.Info("订单响应回写执行失败");
            }
            #endregion
        }
        //订单拒绝响应
        public void HZORDERNOTRESPONSE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("订单拒绝响应开始......");

            #region 拒绝响应
            //1 调用ERP接口获取订单响应信息
            List<Order> bill = erpProxy.GetResponseOrderInfo(branchid, platid, 20, 3);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<Order> bill1 = tpProxy.ResponseBill(branchid, platid, bill, 1);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateOrder(bill1))
            {
                log.Info("订单响应回写执行成功");
            }
            else
            {
                log.Info("订单响应回写执行失败");
            }
            #endregion
        }
        #endregion

        #region 配送单

        //配送单上传
        public void HZPURCHASE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("配送单上传开始......");
            //1 调用ERP接口获取配送单信息
            ////获取还未上传的配送单
            //List<DeliveryBill> bill = erpProxy.GetDeliveryBill(branchid, tpid, 20,0);
            //string ddmxbhs = "";
            ////根据获取的配送单编码获取对应的发票
            //List<DebitNote> note = erpProxy.GetDebitNoteBymxbhs(ddmxbhs);
            List<DeliveryAndDebitNoteBill> bill = erpProxy.GetDeliveryAndDebitNoteBill(branchid, platid, 20, 1);
            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            //List<DeliveryBill> bill1= tpProxy.UpLoadDeliveryBill(bill, note);
            List<DeliveryBill> bill1 = tpProxy.UpLoadDeliveryAndDebitNoteBill(bill);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.WriteBackDeliveryBillState(bill1))
            {
                log.Info("配送单上传回写执行成功");
            }
            else
            {
                log.Info("配送单上传回写执行失败");
            }
        }
        #endregion

        # region 收货单

        //收货单下载
        public void HZRKORDER()
        {
            int iswc = 0;
            int currentpage = 1;
            //Console.WriteLine("service4 executing ......");
            log.Info("收货单下载开始......");
            if (tpProxy == null)
            {
                return;
            }

            //获取还未提取入库单的配送单
            List<DeliveryBill> bill = erpProxy.GetDeliveryBillNotTQ(branchid, platid, 10);

            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //从远程取回收货单
            List<RKOrder> comp = tpProxy.GetGodownEntry(branchid, platid, bill, DateTime.Now, DateTime.Now, out iswc, currentpage);
            if (comp == null)
            {
                return;
            }
            if (comp.Count == 0)
            {
                return;
            }
            //2 调用ERP接口将收货单写入ERP
            if (erpProxy.InsertGodownEntry(comp))
            {
                log.Info("收货单下载执行成功");
            }
            else
            {
                log.Info("收货单下载执行失败");
            }

            //回写配送单的提取状态
            List<DeliveryBill> billupdate = new List<DeliveryBill>();
            foreach (DeliveryBill bil in bill)
            {
                bil.ISRKTQ = 1;
                billupdate.Add(bil);
            }
            if (erpProxy.WriteBackDeliveryBillState(billupdate))
            {
                log.Info("收货单下载执行成功");
            }
            else
            {
                log.Info("收货单下载执行失败");
            }
        }
        #endregion

        #region 退货单

        //退货单下载
        public void HZRETURNORDER()
        {
            int iswc = 0;
            DateTime date;
            //Console.WriteLine("service4 executing ......");
            log.Info("退货单下载开始......");
            if (tpProxy == null)
            {
                return;
            }

            //从ERP获取当前最大的提交时间
            string a = erpProxy.GetLastTDTJRQ(branchid, platid);
            if (a.Replace("\"", "") != null && a.Replace("\"", "") != "")
            {
                date = DateTime.Parse(a.Replace("\"", ""));
            }
            else
            {
                date = DateTime.Parse("2016-05-01 00:00:00");
            }
            int currentpage = 1;
            DateTime enddate = DateTime.Now;
            while (iswc == 0)
            {
                //从远程取回退货单
                List<ReturnOrder> comp = tpProxy.GetReturnOrder(branchid, platid, date, enddate, out iswc, currentpage);
                if (comp == null)
                {
                    return;
                }
                if (comp.Count == 0)
                {
                    return;
                }
                //2 调用ERP接口将退货单写入ERP
                if (erpProxy.UploadReturnOrder(comp))
                {
                    log.Info("退货单下载执行成功");
                }
                else
                {
                    log.Info("退货单下载执行失败");
                    return;
                }
                currentpage += 10;
            }
        }
        //退货单响应
        public void HZRETURNORDERRESPONSE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("退货单响应开始......");
            //1 调用ERP接口获取退货单响应信息
            List<ReturnOrder> bill = erpProxy.GetResponseReturnOrderInfo(branchid, platid, 20, 1);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台响应
            List<ReturnOrder> bill1 = tpProxy.ResponseReturnOrder(branchid, platid, bill, 0);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateReturnOrder(bill1))
            {
                log.Info("退货单响应回写执行成功");
            }
            else
            {
                log.Info("退货单响应回写执行失败");
            }
        }
        //退货单拒绝响应
        public void HZRETURNORDERNOTRESPONSE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("退货单响应开始......");
            //1 调用ERP接口获取退货单响应信息
            List<ReturnOrder> bill = erpProxy.GetResponseReturnOrderInfo(branchid, platid, 20, 3);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台响应
            List<ReturnOrder> bill1 = tpProxy.ResponseReturnOrder(branchid, platid, bill, 1);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateReturnOrder(bill1))
            {
                log.Info("退货单响应回写执行成功");
            }
            else
            {
                log.Info("退货单响应回写执行失败");
            }
        }
        #endregion

        #region 发票
        //退货发票上传
        public void HZRETURNDEBITNOTE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("厂家信息开始......");
            //1 调用ERP接口获取发票信息
            List<DebitNote> note = erpProxy.GetDebitNote(branchid, platid, 20, 0, 1);

            if (tpProxy == null)
            {
                return;
            }
            if (note == null || note.Count == 0)
            {
                return;
            }
            //向平台上传
            List<DebitNote> bill1 = tpProxy.UpLoadDebitNote(note);
            if (bill1 == null)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.WriteBackDebitState(bill1))
            {
                log.Info("回写执行成功");
            }
            else
            {
                log.Info("回写执行失败");
            }
        }
        #endregion

        #endregion


        #region 河南

        #region 测试框架用

        //河南test
        public void HNTEST()
        {
            tpProxy.Test(branchid, platid);
            erpProxy.Test(branchid, platid);
        }
        #endregion

        #region 基础资料
        //河南商品资料
        public void HNGOOD()
        {
            if (DateTime.Now.Hour < 5)
                return;
            int iswc = 0;
            int currentpage = 1;
            //log.Info("河南商品资料开始......");
            if (tpProxy == null)
            {
                return;
            }
            //获取页码数
            //string d = date.Year + "-" + (date.Month + a) + "-" + "01";
            string d = "2016-01-01";
            //string d = "0001/1/1";
            int count = erpProxy.GetProductInfoCount(branchid, platid, d);
            if (count <= 0)
            {
                currentpage = 1;
            }
            else
            {
                currentpage = count / 100 + 1;
            }
            while (iswc == 0)
            {
                string mon = "";
                //1 从远程取回商品资料
                List<Goods> goods = tpProxy.GetProductInfo(branchid, platid, mon, currentpage, out iswc);
                if (goods == null)
                {
                    log.Info("河南商品资料:获取平台空记录");
                    return;
                }
                //2 调用ERP接口将商品资料写入ERP
                if (erpProxy.InsertProductInfo(goods))
                {
                    log.Info("页码:" + currentpage + " 商品资料执行成功");
                    currentpage++;
                }
                else
                {
                    log.Info("页码:" + currentpage + "商品资料执行失败");
                }
            }
        }

        //医院信息
        public void HNHOSPITAL()
        {
            if (DateTime.Now.Hour < 5)
                return;
            int iswc = 0;
            int currentpage = 1;
            //Console.WriteLine("service2 executing ......");
            //log.Info("河南医院信息开始......");
            #region zhengshi

            if (tpProxy == null)
            {
                return;
            }

            //获取页码数
            //string d = date.Year + "-" + (date.Month + a) + "-" + "01";
            //string d = "2016-01-01";
            int count = erpProxy.GetHospitalInfoCount(branchid, platid);
            if (count == -1)
            {
                return;
            }
            if (count <= 0)
            {
                currentpage = 1;
            }
            else
            {
                currentpage = count / 100 + 1;
            }
            while (iswc == 0)
            {
                string mon = "";
                //从远程取回医院资料
                List<Hospital> hos = tpProxy.GetHospitalInfo(branchid, platid, mon, currentpage, out iswc);
                if (hos == null)
                {
                    return;
                }
                //2 调用ERP接口将医院资料写入ERP
                if (erpProxy.InsertHospitalInfo(hos))
                {
                    log.Info("页码:" + currentpage + "医院资料执行成功");
                }
                else
                {
                    log.Info("页码:" + currentpage + "医院资料执行失败");
                }
            }
            #endregion
        }
        #endregion

        #region 订单
        //订单下载
        public void HNORDER()
        {
            if (DateTime.Now.Hour < 5)
                return;
            int iswc = 0;
            DateTime date;
            int currentpage = 1;
            //Console.WriteLine("service4 executing ......");
            //log.Info("订单下载开始......");
            if (tpProxy == null)
            {
                return;
            }
            //从ERP获取当前最大的提交时间
            //string a = erpProxy.GetLastDDTJRQ(branchid, tpid);


            //从本地配置文件获取请求参数
            HNConfig = DeserializeXml(@"Config\HNConfig.xml", typeof(HNConfig)) as HNConfig;
            string a = HNConfig.ORDERLASTTIME.Replace("\"", "");
            currentpage = int.Parse(HNConfig.ORDERCURRENTPAGE);
            date = DateTime.Parse(a);


            //DateTime endTime = DateTime.Now.AddDays(1);
            DateTime endTime = DateTime.Now;

            while (iswc == 0)
            {
                //从远程取回订单
                List<Order> comp = tpProxy.GetOrder(branchid, platid, date, endTime, out iswc, currentpage);
                if (comp == null)
                {
                    return;
                }
                //2 调用ERP接口将厂家资料写入ERP
                if (erpProxy.UploadOrder(comp))
                {
                    //HNConfig.ORDERLASTTIME = endTime.AddDays(-1).ToString();
                    HNConfig.ORDERLASTTIME = endTime.ToString();
                    HNConfig.ORDERCURRENTPAGE = "1";
                    log.Info("订单下载ERP执行成功");
                }
                else
                {
                    HNConfig.ORDERLASTTIME = date.ToString();
                    HNConfig.ORDERCURRENTPAGE = currentpage.ToString();
                    log.Info("订单下载ERP执行失败");
                    break;
                }
                SerializeXML(HNConfig, @"Config\HNConfig.xml");
                currentpage++;
            }
        }
        #endregion

        #region 配送单
        //配送单上传
        public void HNPURCHASE1()
        {
            if (DateTime.Now.Hour < 5)
                return;
            //Console.WriteLine("service4 executing ......");
            //log.Info("配送单上传开始......");
            //1 调用ERP接口获取配送单信息
            ////获取还未上传的配送单
            //List<DeliveryBill> bill = erpProxy.GetDeliveryBill(branchid, tpid, 20, 0);
            //string ddmxbhs = "";
            ////根据获取的配送单编码获取对应的发票
            //List<DebitNote> note = erpProxy.GetDebitNoteBymxbhs(ddmxbhs);
            List<DeliveryAndDebitNoteBill> bill = erpProxy.GetDeliveryAndDebitNoteBill(branchid, platid, 20, 1);
            //List<Array> bill = erpProxy.GetDeliveryAndDebitNoteBill(branchid, tpid, 20);
            if (tpProxy == null)
            {
                return;
            }
            if (bill == null)
            {
                return;
            }
            //向平台上传
            List<DeliveryBill> bill1 = tpProxy.UpLoadDeliveryAndDebitNoteBill(bill);
            if (bill1 == null)
            {
                return;
            }
            //根据ddmxbh等条件获取配送表数据

            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.WriteBackDeliveryBillState(bill1))
            {
                log.Info("配送单上传回写执行成功");
            }
            else
            {
                log.Info("配送单上传回写执行失败");
            }
        }
        public void HNPURCHASE()
        {
            if (DateTime.Now.Hour < 5)
                return;
            //Console.WriteLine("service4 executing ......");
            //log.Info("配送单上传开始......");
            //1 调用ERP接口获取配送单信息
            ////获取还未上传的配送单
            //List<DeliveryBill> bill = erpProxy.GetDeliveryBill(branchid, tpid, 20, 0);
            //string ddmxbhs = "";
            ////根据获取的配送单编码获取对应的发票
            //List<DebitNote> note = erpProxy.GetDebitNoteBymxbhs(ddmxbhs);
            //List<DeliveryAndDebitNoteBill> bill = erpProxy.GetDeliveryAndDebitNoteBill(branchid, tpid, 20, 1);
            List<DeliveryAndDebitNoteBill1> bill = erpProxy.GetDeliveryAndDebitNoteBill(branchid, platid, 2);
            if (tpProxy == null)
            {
                return;
            }
            if (bill == null)
            {
                return;
            }
            if (bill.Count <= 0)
            {
                return;
            }
            //向平台上传
            string bill1 = tpProxy.UpLoadDeliveryAndDebitNoteBill(bill);
            if (bill1 == null)
            {
                return;
            }
            //根据ddmxbh等条件获取配送表数据

            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.WriteBackDeliveryBillState(bill1, 2) > 0)
            {
                log.Info("配送单上传回写执行成功");
            }
            else
            {
                log.Info("配送单上传回写执行失败");
            }
        }
        #endregion

        #region 收货单--没用
        //收货单下载
        public void HNRKORDER()
        {
            if (DateTime.Now.Hour < 5)
                return;
            int iswc = 0;
            DateTime date;
            int currentpage = 1;
            //Console.WriteLine("service4 executing ......");
            //log.Info("收货单下载开始......");
            if (tpProxy == null)
            {
                return;
            }

            //获取还未提取入库单的配送单
            //List<DeliveryBill> bill = erpProxy.GetDeliveryBillNotTQ(branchid, tpid, 1);
            List<DeliveryBill> bill = null;
            //从本地配置文件获取请求参数
            HNConfig = DeserializeXml(@"Config\HNConfig.xml", typeof(HNConfig)) as HNConfig;
            string a = HNConfig.RKORDERLASTTIME.Replace("\"", "");
            currentpage = int.Parse(HNConfig.RKORDERCURRENTPAGE);
            date = DateTime.Parse(a);
            DateTime endTime = DateTime.Now.AddDays(1);
            //
            while (iswc == 0)
            {
                //从远程取回收货单
                List<RKOrder> comp = tpProxy.GetGodownEntry(branchid, platid, bill, date, endTime, out iswc, currentpage);
                if (comp == null)
                {
                    return;
                }
                //2 调用ERP接口将收货单写入ERP
                if (erpProxy.InsertGodownEntry(comp))
                {
                    //HNConfig.RKORDERLASTTIME = endTime.ToString();
                    //HNConfig.RKORDERCURRENTPAGE = "1";
                    log.Info("收货单下载执行成功");
                }
                else
                {
                    //HNConfig.RKORDERLASTTIME = date.ToString();
                    //HNConfig.RKORDERCURRENTPAGE = currentpage.ToString();
                    log.Info("收货单下载执行失败");
                    break;
                }
                //SerializeXML(HNConfig, @"Config\HNConfig.xml");
                currentpage++;
            }
            //回写配送单的提取状态
            //List<DeliveryBill> billupdate = new List<DeliveryBill>();
            //foreach (DeliveryBill bil in bill)
            //{
            //    DeliveryBill b = new DeliveryBill();
            //    b = bil;
            //    b.ISRKTQ = 1;
            //    billupdate.Add(b);
            //}
            //if (erpProxy.WriteBackDeliveryBillState(billupdate))
            //{
            //    log.Info("收货单下载执行成功");
            //}
            //else
            //{
            //    log.Info("收货单下载执行失败");
            //}
        }

        #endregion

        #endregion


        #region 长春

        #region 测试框架用

        //长春test
        public void CCTEST()
        {
            tpProxy.Test(branchid, platid);
            erpProxy.Test(branchid, platid);
        }
        #endregion


        #region 基础资料
        //长春商品资料
        public void CCGOOD()
        {
            if (DateTime.Now.Hour < 5)
                return;
            int iswc = 0;
            if (tpProxycc == null)
            {
                return;
            }
            while (iswc == 0)
            {
                List<Tb_Drugs> list1 = new List<Tb_Drugs>();
                //1 从远程取回商品资料
                List<Goods> goods = tpProxycc.GetProductInfo(branchid, platid, "", 0, out iswc, out list1);
                if (goods == null)
                {
                    log.Info("商品资料:获取平台空记录");
                    return;
                }
                //2 调用ERP接口将商品资料写入ERP
                if (erpProxy.InsertProductInfo(goods))
                {
                    log.Info("商品资料执行成功");
                }
                else
                {
                    log.Info("商品资料执行失败");
                }
                //3 回写平台状态
                if (tpProxycc.UpdateProductInfo(list1))
                {
                    log.Info("商品资料回写成功");
                }
                else
                {
                    log.Info("商品资料回写失败");
                }
            }
        }

        //医院信息
        public void CCHOSPITAL()
        {
            if (DateTime.Now.Hour < 5)
                return;
            int iswc = 0;

            if (tpProxycc == null)
            {
                return;
            }
            while (iswc == 0)
            {
                List<Tb_Hospital> list1 = new List<Tb_Hospital>();
                //从远程取回医院资料
                List<Hospital> hos = tpProxycc.GetHospitalInfo(branchid, platid, "", 0, out iswc, out list1);
                if (hos == null)
                {
                    log.Info("医院信息:获取平台空记录");
                    return;
                }
                //2 调用ERP接口将医院资料写入ERP
                if (erpProxy.InsertHospitalInfo(hos))
                {
                    log.Info("医院资料执行成功");
                }
                else
                {
                    log.Info("医院资料执行失败");
                }
                //3 回写平台状态
                if (tpProxycc.UpdateHospitalInfo(list1))
                {
                    log.Info("医院资料回写成功");
                }
                else
                {
                    log.Info("医院资料回写失败");
                }
            }
        }
        #endregion

        #region 订单
        public void CCORDER()
        {
            int iswc = 0;
            DateTime date = DateTime.Now;
            //int currentpage = 1;
            if (tpProxycc == null)
            {
                return;
            }
            List<Tb_Orderdetail> list1 = new List<Tb_Orderdetail>();
            //从远程取回订单
            List<Order> comp = tpProxycc.GetOrder(branchid, platid, date, DateTime.Now, out iswc, 0, out list1);
            if (comp == null)
            {
                return;
            }
            //2 调用ERP接口将订单写入ERP
            if (erpProxy.UploadOrder(comp))
            {
                log.Info("订单下载ERP执行成功");
            }
            else
            {
                log.Info("订单下载ERP执行失败");
                return;
            }
            //3 回写平台状态
            if (tpProxycc.UpdateOrder(list1))
            {
                log.Info("订单平台回写成功");
            }
            else
            {
                log.Info("订单平台回写失败");
            }
        }
        #endregion

        #region 退货单  未测试

        //退货单下载
        public void CCRETURNORDER()
        {
            int iswc = 0;
            DateTime date = DateTime.Now;
            if (tpProxycc == null)
            {
                return;
            }

            List<Tb_Retruninfo> list1 = new List<Tb_Retruninfo>();
            //从远程取回退货单
            List<ReturnOrder> comp = tpProxycc.GetReturnOrder(branchid, platid, date, DateTime.Now, out iswc, 0, out list1);
            if (comp == null)
            {
                return;
            }
            //2 调用ERP接口将退货单写入ERP
            if (erpProxy.UploadReturnOrder(comp))
            {
                log.Info("退货单下载ERP执行成功");
            }
            else
            {
                log.Info("退货单下载ERP执行失败");
            }
            //3 回写平台状态
            if (tpProxycc.UpdateReturnInfo(list1))
            {
                log.Info("订单平台回写成功");
            }
            else
            {
                log.Info("订单平台回写失败");
            }
        }
        //退货单响应
        public void CCRETURNORDERRESPONSE()
        {
            //Console.WriteLine("service4 executing ......");
            //log.Info("退货单响应开始......");
            //1 调用ERP接口获取退货单响应信息
            List<ReturnOrder> bill = erpProxy.GetResponseReturnOrderInfo(branchid, platid, 20, 1);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台响应
            List<ReturnOrder> bill1 = tpProxy.ResponseReturnOrder(branchid, platid, bill, 0);
            if (bill1 == null)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateReturnOrder(bill1))
            {
                log.Info("退货单响应回写ERP执行成功");
            }
            else
            {
                log.Info("退货单响应回写ERP执行失败");
            }
        }
        //退货单拒绝响应
        public void CCRETURNORDERNOTRESPONSE()
        {
            //Console.WriteLine("service4 executing ......");
            //log.Info("退货单响应开始......");
            //1 调用ERP接口获取退货单响应信息
            List<ReturnOrder> bill = erpProxy.GetResponseReturnOrderInfo(branchid, platid, 20, 3);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台响应
            List<ReturnOrder> bill1 = tpProxy.ResponseReturnOrder(branchid, platid, bill, 1);
            if (bill1 == null)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateReturnOrder(bill1))
            {
                log.Info("退货单响应回写ERP执行成功");
            }
            else
            {
                log.Info("退货单响应回写ERP执行失败");
            }
        }
        #endregion

        #region 配送单
        //配送单上传
        public void CCPURCHASE()
        {
            //Console.WriteLine("service4 executing ......");
            //log.Info("配送单上传开始......");
            //1 调用ERP接口获取配送单信息
            ////获取还未上传的配送单
            //List<DeliveryBill> bill = erpProxy.GetDeliveryBill(branchid, tpid, 20, 0);
            //string ddmxbhs = "";
            ////根据获取的配送单编码获取对应的发票
            //List<DebitNote> note = erpProxy.GetDebitNoteBymxbhs(ddmxbhs);
            List<DeliveryAndDebitNoteBill> bill = erpProxy.GetDeliveryAndDebitNoteBill(branchid, platid, 200, 1);
            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<DeliveryBill> bill1 = tpProxy.UpLoadDeliveryAndDebitNoteBill(bill);
            if (bill1 == null)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.WriteBackDeliveryBillState(bill1))
            {
                log.Info("配送单上传回写执行成功");
            }
            else
            {
                log.Info("配送单上传回写执行失败");
            }
        }
        #endregion


        #region 库存  没用

        public void CCSTOCK()
        {
            //Console.WriteLine("service4 executing ......");
            //log.Info("配送单上传开始......");
            //1 调用ERP接口获取配送单信息
            ////获取还未上传的配送单
            //List<DeliveryBill> bill = erpProxy.GetDeliveryBill(branchid, tpid, 20, 0);
            //string ddmxbhs = "";
            ////根据获取的配送单编码获取对应的发票
            //List<DebitNote> note = erpProxy.GetDebitNoteBymxbhs(ddmxbhs);
            List<Stock> bill = erpProxy.GetStock("");
            //List<Stock> bill = erpProxy.GetStock(branchid, tpid, 20, 0);
            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<Stock> bill1 = tpProxy.UpLoadStock(bill);
            if (bill1 == null)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.WriteBackStockState(bill1))
            {
                log.Info("配送单上传回写执行成功");
            }
            else
            {
                log.Info("配送单上传回写执行失败");
            }
        }
        #endregion

        #endregion


        #region 上海器械

        #region 医院及配送点
        //医院信息
        public void SHQXHOSPITAL()
        {
            #region zhengshi
            if (tpProxy == null)
            {
                return;
            }
            //从ERP获取医院信息 返回实体集合
            List<Hospital> hospitals = erpProxy.GetHospitalInfo(branchid, platid, 0, 200);
            if (hospitals == null || hospitals.Count == 0)
            {
                return;
            }
            //从远程取回医院资料
            List<HospitalPSD> hos = tpProxy.GetHospitalPSDInfo(branchid, platid, hospitals);
            if (hos == null)
            {
                return;
            }
            if (hos.Count == 0)
            {
                return;
            }
            //2 调用ERP接口将医院资料写入ERP
            if (erpProxy.InsertHospitalPSDInfo(hos))
            {
                log.Info("医院资料执行成功");
            }
            else
            {
                log.Info("医院资料执行失败");
            }

            //3 调用ERP接口回写获取的状态
            List<Hospital> hospitalsback = new List<Hospital>();
            foreach (Hospital item in hospitals)
            {
                item.IS_SHJ = "1";
                hospitalsback.Add(item);
            }
            if (erpProxy.InsertHospitalInfo(hospitalsback))
            {
                log.Info("医院资料回写状态成功");
            }
            else
            {
                log.Info("医院资料回写状态失败");
            }
            #endregion
        }

        //将医院信息的是否获取配送点状态重置
        public void SHQXUPDATEHOSPITAL()
        {
            #region zhengshi
            if (tpProxy == null)
            {
                return;
            }
            //从ERP获取医院信息 返回实体集合
            List<Hospital> hospitals = erpProxy.GetHospitalInfo(branchid, platid, 1, 1000);
            if (hospitals == null || hospitals.Count == 0)
            {
                return;
            }

            //调用ERP接口回写获取的状态
            List<Hospital> hospitalsback = new List<Hospital>();
            foreach (Hospital item in hospitals)
            {
                item.IS_SHJ = "0";
                hospitalsback.Add(item);
            }
            if (erpProxy.InsertHospitalInfo(hospitalsback))
            {
                log.Info("医院资料重置状态成功");
            }
            else
            {
                log.Info("医院资料重置状态失败");
            }
            #endregion
        }
        #endregion

        #region 库存

        //库存上报
        public void SHQXSTOCK()
        {
            #region zhengshi
            if (tpProxy == null)
            {
                return;
            }
            //从ERP获取医院信息 返回实体集合
            List<Stock> bill = erpProxy.GetStock("");
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<Stock> bill1 = tpProxy.UpLoadStock(bill);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.WriteBackStockState(bill1))
            {
                log.Info("配送单上传回写执行成功");
            }
            else
            {
                log.Info("配送单上传回写执行失败");
            }

            #endregion
        }
        #endregion

        #region 订单

        //订单下载
        public void SHQXORDER()
        {
            int iswc = 0;

            log.Info("订单下载开始......");
            if (tpProxy == null)
            {
                return;
            }
            //从本地配置文件获取请求参数
            SHQXConfig = DeserializeXml(@"Config\SHQXConfig.xml", typeof(SHQXConfig)) as SHQXConfig;
            string a = SHQXConfig.lastddbh.Replace("\"", "");
            string returnlastzxspbh = "";
            while (iswc == 0)
            {
                if (returnlastzxspbh != "")
                    a = returnlastzxspbh;
                //从远程取回订单
                List<Order> comp = tpProxy.GetOrder(branchid, platid, a, out iswc, out returnlastzxspbh);
                if (comp == null)
                {
                    return;
                }
                if (comp.Count == 0)
                {
                    return;
                }
                //2 调用ERP接口将厂家资料写入ERP
                if (erpProxy.UploadOrder(comp))
                {
                    SHQXConfig.lastddbh = returnlastzxspbh;
                    log.Info("订单下载ERP执行成功");
                }
                else
                {
                    log.Info("订单下载执行失败");
                    return;
                }
                SerializeXML(SHQXConfig, @"Config\SHQXConfig.xml");
            }
        }

        //订单响应
        public void SHQXORDERRESPONSE()
        {
            log.Info("订单响应开始......");
            #region 正常响应
            //1 调用ERP接口获取订单响应信息
            List<Order> bill = erpProxy.GetResponseOrderInfo(branchid, platid, 20, 2, 1);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<Order> bill1 = tpProxy.ResponseBill(branchid, platid, bill, 0);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP  回写成功
            if (erpProxy.UpdateOrder(bill1))
            {
                log.Info("订单响应回写执行成功");
            }
            else
            {
                log.Info("订单响应回写执行失败");
            }
            #endregion
        }

        //订单完成后确认(上传配送单之后操作)
        public void SHQXORDEROVERQUEREN()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("订单响应开始......");
            #region 正常响应
            //1 调用ERP接口获取订单响应信息
            List<Order> bill = erpProxy.GetResponseOrderInfo(branchid, platid, 20, 6);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<Order> bill1 = tpProxy.OverBillOK(branchid, platid, bill);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateOrder(bill1))
            {
                log.Info("订单响应回写执行成功");
            }
            else
            {
                log.Info("订单响应回写执行失败");
            }
            #endregion
        }

        //订单自填上传
        public void SHQXORDERWRITE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("订单响应开始......");
            #region 正常响应
            //1 调用ERP接口获取订单响应信息
            List<Order> bill = erpProxy.GetDTOrderInfo(branchid, platid, 0, 1, 0);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<Order> bill1 = tpProxy.WriteBill(branchid, platid, bill, 0);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateOrder(bill1))
            {
                log.Info("订单响应回写执行成功");
            }
            else
            {
                log.Info("订单响应回写执行失败");
            }
            #endregion
        }

        //订单自填作废
        public void SHQXORDERWRITE1()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("订单响应开始......");
            #region 正常响应
            //1 调用ERP接口获取订单响应信息
            List<Order> bill = erpProxy.GetDTOrderInfo(branchid, platid, 2, 1, 1);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<Order> bill1 = tpProxy.WriteBill(branchid, platid, bill, 1);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateOrder(bill1))
            {
                log.Info("订单响应回写执行成功");
            }
            else
            {
                log.Info("订单响应回写执行失败");
            }
            #endregion
        }

        //订单自填确认
        public void SHQXORDERWRITEQUEREN()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("订单响应开始......");
            #region 正常响应
            //1 调用ERP接口获取订单响应信息
            List<Order> bill = erpProxy.GetDTOrderQueRenInfo(branchid, platid, 1, 20, 2);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            string ddbhs = "";
            //向平台上传
            List<Order> bill1 = tpProxy.ReadBill(branchid, platid, bill, out ddbhs);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            List<Order> bill2 = erpProxy.GetDTOrderQueRenInfo(branchid, platid, 1, ddbhs.Substring(0, ddbhs.Length - 1));
            if (bill2 == null)
            {
                return;
            }
            if (bill2.Count < 0)
            {
                return;
            }
            List<Order> bill3 = new List<Order>();
            foreach (Order dr in bill1)
            {
                List<Order> bil = bill2.FindAll(delegate (Order p)
                { return p.DDBH == dr.DDBH.ToString().Trim(); });
                foreach (Order dr1 in bil)
                {
                    dr1.IS_SC = dr.IS_SC;
                    dr1.ZTCLJG = dr.ZTCLJG;
                    dr1.CWXX = dr.CWXX;
                    dr1.BZXX = dr.BZXX;
                    bill3.Add(dr1);
                }
            }

            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateOrder(bill3))
            {
                log.Info("订单响应回写执行成功");
            }
            else
            {
                log.Info("订单响应回写执行失败");
            }
            //}
            #endregion
        }
        #endregion

        #region 退单
        //退单下载
        public void SHQXRETURNORDER()
        {
            int iswc = 0;

            log.Info("退货单下载开始......");
            if (tpProxy == null)
            {
                return;
            }
            //从本地配置文件获取请求参数
            SHQXConfig = DeserializeXml(@"Config\SHQXConfig.xml", typeof(SHQXConfig)) as SHQXConfig;
            string a = SHQXConfig.lasttdbh.Replace("\"", "");
            string returnlasttdbh = "";
            while (iswc == 0)
            {
                if (returnlasttdbh != "")
                    a = returnlasttdbh;
                //从远程取回订单
                List<ReturnOrder> comp = tpProxy.GetReturnOrder(branchid, platid, a, out iswc, out returnlasttdbh);
                if (comp == null)
                {
                    return;
                }
                if (comp.Count == 0)
                {
                    return;
                }
                //2 调用ERP接口将厂家资料写入ERP
                if (erpProxy.UploadReturnOrder(comp))
                {
                    log.Info("退货单下载执行成功");
                    SHQXConfig.lasttdbh = returnlasttdbh;
                    log.Info("退货单下载ERP执行成功");
                }
                else
                {
                    log.Info("退货单下载执行失败");
                    return;
                }
                SerializeXML(SHQXConfig, @"Config\SHQXConfig.xml");
            }
        }

        //退单响应
        public void SHQXRETURNORDERRESPONSE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("退货单响应开始......");
            #region 正常响应
            //1 调用ERP接口获取订单响应信息
            List<ReturnOrder> bill = erpProxy.GetResponseReturnOrderInfo(branchid, platid, 20, 2, 1);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<ReturnOrder> bill1 = tpProxy.ResponseReturnOrder(branchid, platid, bill, 0);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateReturnOrder(bill1))
            {
                log.Info("退货单响应回写执行成功");
            }
            else
            {
                log.Info("退货单响应回写执行失败");
            }
            #endregion
        }

        //退单自填上传
        public void SHQXRETURNORDERWRITE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("退货单响应开始......");
            #region 正常响应
            //1 调用ERP接口获取订单响应信息
            List<ReturnOrder> bill = erpProxy.GetDTReturnOrderInfo(branchid, platid, 0, 1, 0);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<ReturnOrder> bill1 = tpProxy.WriteReturnBill(branchid, platid, bill, 0);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateReturnOrder(bill1))
            {
                log.Info("退货单响应回写执行成功");
            }
            else
            {
                log.Info("退货单响应回写执行失败");
            }
            #endregion
        }

        //退单自填作废
        public void SHQXRETURNORDERWRITE1()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("退货单响应开始......");
            #region 正常响应
            //1 调用ERP接口获取订单响应信息
            List<ReturnOrder> bill = erpProxy.GetDTReturnOrderInfo(branchid, platid, 2, 1, 1);

            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<ReturnOrder> bill1 = tpProxy.WriteReturnBill(branchid, platid, bill, 1);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.UpdateReturnOrder(bill1))
            {
                log.Info("订单响应回写执行成功");
            }
            else
            {
                log.Info("订单响应回写执行失败");
            }
            #endregion
        }

        //退单自填确认
        public void SHQXRETURNORDERWRITEQUEREN()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("退货单响应开始......");
            #region 正常响应
            //1 调用ERP接口获取订单响应信息
            List<ReturnOrder> bill = erpProxy.GetDTReturnOrderQueRenInfo(branchid, platid, 1, 20, 2);


            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            string thdbhs = "";
            //向平台上传
            List<ReturnOrder> bill1 = tpProxy.QueRenReturnBill(branchid, platid, bill, out thdbhs);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            List<ReturnOrder> bill2 = erpProxy.GetAllReturnOrderProperty(branchid, platid, thdbhs.Substring
                (0, thdbhs.Length - 1), 1);
            if (bill2 == null)
            {
                return;
            }
            if (bill2.Count < 0)
            {
                return;
            }
            List<ReturnOrder> bill3 = new List<ReturnOrder>();
            foreach (ReturnOrder dr in bill1)
            {
                List<ReturnOrder> bil = bill2.FindAll(p => p.THDBH == dr.THDBH);
                foreach (ReturnOrder dr1 in bil)
                {
                    dr1.IS_SC = dr.IS_SC;
                    dr1.ZTCLJG = dr.ZTCLJG;
                    dr1.CWXX = dr.CWXX;
                    dr1.BZXX = dr.BZXX;
                    bill3.Add(dr1);
                }
            }

            //3调用ERP接口将返回状态写入ERP 
            if (erpProxy.UpdateReturnOrder(bill3))
            {
                log.Info("订单响应回写执行成功");
            }
            else
            {
                log.Info("订单响应回写执行失败");
            }
            #endregion
        }
        #endregion

        #region 配送单-----------------------------------------------------------------------------------

        //配送单上传
        public void SHQXPURCHASE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("配送单上传开始......");
            if (tpProxy == null)
            {
                return;
            }
            //1 先找出主单单据编号
            List<string> data = erpProxy.GetGroupPSDH(branchid, platid, 20, 1);
            foreach (string psdh in data)
            {
                //根据主单编号获取对应的明细信息
                //1 调用ERP接口获取配送单信息
                List<DeliveryBill> bill = erpProxy.GetDeliveryBillByPSDH(branchid, platid, psdh);

                if (bill == null || bill.Count == 0)
                {
                    continue;
                }
                //向平台上传
                List<DeliveryBill> bill1 = tpProxy.UpLoadDeliveryBill(bill, null);
                if (bill1 == null)
                {
                    continue;
                }
                if (bill1.Count == 0)
                {
                    continue;
                }
                //3调用ERP接口将返回状态写入ERP
                if (erpProxy.WriteBackDeliveryBillState(bill1))
                {
                    log.Info("配送单上传回写执行成功");
                }
                else
                {
                    log.Info("配送单上传回写执行失败");
                }
            }
        }


        //配送单作废
        public void SHQXPURCHASE1()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("配送单上传开始......");
            //1 调用ERP接口获取配送单信息
            List<DeliveryBill> bill = erpProxy.GetDeliveryBill(branchid, platid, 20, 0);
            if (tpProxy == null)
            {
                return;
            }
            if (bill == null || bill.Count == 0)
            {
                return;
            }
            //向平台上传
            List<DeliveryBill> bill1 = tpProxy.UpLoadDeliveryBill1(bill, null);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.WriteBackDeliveryBillState(bill1))
            {
                log.Info("配送单上传回写执行成功");
            }
            else
            {
                log.Info("配送单上传回写执行失败");
            }
        }

        //配送单上传确认 (取单笔配送单下的所有明细)
        public void SHQXPURCHASEQUEREN()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("配送单上传开始......");
            if (tpProxy == null)
            {
                return;
            }
            //1 先找出主单单据编号
            List<string> data = erpProxy.GetGroupPSDBH(branchid, platid, 20, 2);
            if (data == null)
            {
                return;
            }
            foreach (string psdbh in data)
            {
                //根据主单编号获取对应的明细信息
                //1 调用ERP接口获取配送单信息
                List<DeliveryBill> bill = erpProxy.GetDeliveryBillByPSDBH(branchid, platid, psdbh);
                if (bill == null || bill.Count == 0)
                {
                    continue;
                }
                //向平台上传
                List<DeliveryBill> bill1 = tpProxy.WriteBackDeliveryBill(bill);
                if (bill1 == null)
                {
                    continue;
                }
                if (bill1.Count == 0)
                {
                    continue;
                }
                //3调用ERP接口将返回状态写入ERP  会写成功
                if (erpProxy.WriteBackDeliveryBillState(bill1))
                {
                    log.Info("配送单上传回写执行成功");
                }
                else
                {
                    log.Info("配送单上传回写执行失败");
                }
            }
        }


        //配送单明细状态获取
        public void SHQXPURCHASESTATUS()
        {
            int iswc = 0;
            //Console.WriteLine("service4 executing ......");
            log.Info("配送单上传开始......");
            if (tpProxy == null)
            {
                return;
            }
            //1 调用ERP接口获取配送单信息
            List<string> bill = erpProxy.GetGroupPSDBH(branchid, platid, 20, "30,01");
            /* List<string> bill = new List<string>(); *///20170420010000003591
            if (bill == null)
            {
                return;
            }
            foreach (string psdbh in bill)
            {
                iswc = 0;
                ////根据获取的配送单编码获取对应的发票
                List<DeliveryBill> note = erpProxy.GetDeliveryBillByPSDBH(branchid, platid, psdbh);
                if (note == null || note.Count == 0)
                {
                    continue;
                }
                //SHQXConfig = DeserializeXml(@"Config\SHQXConfig.xml", typeof(SHQXConfig)) as SHQXConfig;
                //string lastpsdbh = SHQXConfig.lastpsmxbh;
                string lastpsdbh = "";
                string outlastbh = "";
                while (iswc == 0)
                {
                    if (outlastbh != "")
                        lastpsdbh = outlastbh;
                    //向平台上传
                    List<DeliveryBill> bill1 = tpProxy.GetDeliveryBillStatus(note, lastpsdbh, out outlastbh, out iswc);
                    if (bill1 == null)
                    {
                        return;
                    }
                    if (bill1.Count == 0)
                    {
                        return;
                    }
                    //3调用ERP接口将返回状态写入ERP
                    if (erpProxy.WriteBackDeliveryBillState(bill1))
                    {
                        //SHQXConfig.lastpsmxbh = outlastbh;
                        log.Info("配送单上传回写执行成功");
                    }
                    else
                    {
                        log.Info("配送单上传回写执行失败");
                    }
                    //SerializeXML(SHQXConfig, @"Config\SHQXConfig.xml");
                }
            }
        }
        #endregion

        #region 发票-----------------------------------------------------------------------------------

        //发票上传
        public void SHQXDEBITNOTE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("发票单上传开始......");

            if (tpProxy == null)
            {
                return;
            }
            //1 先找出主单单据编号
            List<string> bill = erpProxy.GetGroupFPH(branchid, platid, 20, 0, -1);
            if (bill == null)
            {
                return;
            }
            foreach (string fph in bill)
            {
                //根据主单编号获取对应的明细信息
                List<DebitNote> note = erpProxy.GetDebitNoteByFPH(branchid, platid, fph);
                if (note == null || note.Count == 0)
                {
                    continue;
                }
                //向平台上传
                List<DebitNote> bill1 = tpProxy.UpLoadDebitNote(note);
                if (bill1 == null)
                {
                    continue;
                }
                if (bill1.Count == 0)
                {
                    continue;
                }
                //3调用ERP接口将返回状态写入ERP 回写成功了
                if (erpProxy.WriteBackDebitState(bill1))
                {
                    log.Info("配送单上传回写执行成功");
                }
                else
                {
                    log.Info("配送单上传回写执行失败");
                }
            }
        }


        //发票作废
        public void SHQXDEBITNOTE1()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("配送单上传开始......");

            ////根据获取的配送单编码获取对应的发票
            List<DebitNote> note = erpProxy.GetDebitNote(branchid, platid, 20, 0, -1);
            if (tpProxy == null)
            {
                return;
            }
            if (note == null || note.Count == 0)
            {
                return;
            }
            //向平台上传
            List<DebitNote> bill1 = tpProxy.UpLoadDebitNote1(note);
            if (bill1 == null)
            {
                return;
            }
            if (bill1.Count == 0)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.WriteBackDebitState(bill1))
            {
                log.Info("配送单上传回写执行成功");
            }
            else
            {
                log.Info("配送单上传回写执行失败");
            }
        }

        //发票确认 (取单笔配送单下的所有明细)
        public void SHQXDEBITNOTEQUEREN()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("配送单上传开始......");
            if (tpProxy == null)
            {
                return;
            }
            //1 先找出主单单据编号
            List<string> bill = erpProxy.GetGroupFPBH(branchid, platid, 20, 1, -1);
            if (bill == null)
            {
                return;
            }
            foreach (string fpbh in bill)
            {
                ////根据获取的配送单编码获取对应的发票
                List<DebitNote> note = erpProxy.GetDebitNoteByFPBH(branchid, platid, fpbh);
                if (note == null || note.Count == 0)
                {
                    continue;
                }
                //向平台上传
                List<DebitNote> bill1 = tpProxy.DebitNoteQenRen(note);
                if (bill1 == null)
                {
                    continue;
                }
                if (bill1.Count == 0)
                {
                    continue;
                }
                //3调用ERP接口将返回状态写入ERP 完成了会写
                if (erpProxy.WriteBackDebitState(bill1))
                {
                    log.Info("配送单上传回写执行成功");
                }
                else
                {
                    log.Info("配送单上传回写执行失败");
                }
            }
        }


        //发票明细状态获取
        public void SHQXDEBITNOTESTATUS()
        {

            int iswc = 0;
            //Console.WriteLine("service4 executing ......");
            log.Info("发票明细获取开始......");

            if (tpProxy == null)
            {
                return;
            }
            ////根据处理类型获取对应的发票
            List<string> bill = erpProxy.GetGroupFPBH(branchid, platid, 20, "30,01,21");
            //List<string> bill = new List<string>();
            //string a = "20170421010000003712";
            //bill.Add(a);
            if (bill == null)
            {
                return;
            }
            foreach (string fpbh in bill)
            {
                iswc = 0;
                ////根据获取的配送单编码获取对应的发票
                List<DebitNote> note = erpProxy.GetDebitNoteByFPBH(branchid, platid, fpbh);
                if (note == null || note.Count == 0)
                {
                    continue;
                }
                //SHQXConfig = DeserializeXml(@"Config\SHQXConfig.xml", typeof(SHQXConfig)) as SHQXConfig;
                //string lastfpbh = SHQXConfig.lastfpbh;
                string lastfpbh = "";
                string outlastbh = "";
                while (iswc == 0)
                {
                    if (outlastbh != "")
                        lastfpbh = outlastbh;
                    //向平台上传
                    List<DebitNote> bill1 = tpProxy.GetDebitNoteStatus(note, lastfpbh, out outlastbh, out iswc);
                    if (bill1 == null)
                    {
                        return;
                    }
                    if (bill1.Count == 0)
                    {
                        return;
                    }
                    //3调用ERP接口将返回状态写入ERP
                    if (erpProxy.WriteBackDebitState(bill1))
                    {
                        //SHQXConfig.lastfpbh = outlastbh;
                        log.Info("发票明细状态回写执行成功");
                    }
                    else
                    {
                        log.Info("发票明细状态回写执行失败");
                    }
                    //SerializeXML(SHQXConfig, @"Config\SHQXConfig.xml");
                }
            }
        }
        #endregion

        #endregion
        

        #region 福建

        #region 测试框架用

        //福建test
        public void FJTEST()
        {
            tpProxy.Test(branchid, platid);
            erpProxy.Test(branchid, platid);
        }
        #endregion

        #region 基础资料
        //福建商品资料
        public void FJGOOD()
        {
            if (DateTime.Now.Hour < 5)
                return;
            int iswc = 0;
            int currentpage = 1;
            //log.Info("河南商品资料开始......");
            if (tpProxy == null)
            {
                return;
            }
            //获取页码数
            //string d = date.Year + "-" + (date.Month + a) + "-" + "01";
            //string d = "2016-01-01";
            //string d = "0001/1/1";
            //string d = "";
            //int count = erpProxy.GetProductInfoAllCount(branchid, platid, "");
            //if (count <= 0)
            //{
            //    currentpage = 1;
            //}
            //else
            //{
            //    currentpage = count / 200 + 1;
            //}
            while (iswc == 0)
            {
                string mon = "";
                //1 从远程取回商品资料
                List<Goods> goods = tpProxy.GetProductInfo(branchid, platid, mon, currentpage, out iswc);
                if (goods == null)
                {
                    log.Info("福建商品资料:获取平台空记录");
                    return;
                }
                //2 调用ERP接口将商品资料写入ERP
                if (erpProxy.InsertProductInfo(goods))
                {
                    log.Info("页码:" + currentpage + " 商品资料执行成功");
                    currentpage++;
                }
                else
                {
                    log.Info("页码:" + currentpage + "商品资料执行失败");
                }
            }
        }

        //福建库存上报
        public void FJSTOCK()
        {
            if (DateTime.Now.Hour < 5)
                return;
            //int iswc = 0;
            //int currentpage = 1;
            #region zhengshi

            if (tpProxy == null)
            {
                return;
            }

            //获取页码数
            //string d = date.Year + "-" + (date.Month + a) + "-" + "01";
            //string d = "2016-01-01";
            List<Stock1> stocks = erpProxy.GetStockFj(branchid,platid,0,200);
            if (stocks.Count < 0 && stocks == null) return;
            List<DrugBaseStock> list = tpProxy.UpLoadStockFJ(stocks);
            if (list.Count <= 0 || list == null) return;
            if (erpProxy.WriteBackStockStateFj(list))
            {
                log.Info("库存回写成功.....");
            }
            else
            {
                log.Info("库存回写失败....");
                return;
            }

            #endregion
        }
        #endregion

        #region 订单
        //订单下载
        public void FJORDER()
        {
            if (DateTime.Now.Hour < 5)
                return;
            int iswc = 0;
            int currentpage = 1;
            int pageNumber1 = 1;
            int pageNumber2 = 1;
            int pageNumber3 = 1;
            //Console.WriteLine("service4 executing ......");
            //log.Info("订单下载开始......");
            if (tpProxy == null)
            {
                return;
            }
            //从ERP获取当前最大的提交时间
            //string a = erpProxy.GetLastDDTJRQ(branchid, tpid);


            //从本地配置文件获取请求参数
            FJConfig = DeserializeXml(@"Config\FJConfig.xml", typeof(FJConfig)) as FJConfig;
            pageNumber1 = int.Parse(FJConfig.ORDERCURRENTPAGE1);
            pageNumber2 = int.Parse(FJConfig.ORDERCURRENTPAGE2);
            pageNumber3 = int.Parse(FJConfig.ORDERCURRENTPAGE3);
            string[] a = { "1","2","3"};
            foreach (string i in a)
            {
                iswc = 0;
                if (i == "1")
                {
                    #region 正常订单
                    while (iswc == 0)
                    {
                        //从远程取回订单
                        List<Order> comp = tpProxy.GetOrder(branchid, platid, out iswc, out currentpage, pageNumber1, i);
                        if (comp == null)
                        {
                            return;
                        }

                        //2 调用ERP接口将厂家资料写入ERP
                        if (erpProxy.UploadOrder(comp))
                        {
                            pageNumber1 = currentpage;
                            FJConfig.ORDERCURRENTPAGE1 = pageNumber1.ToString();
                            log.Info("订单下载ERP执行成功");
                        }
                        else
                        {
                            //pageNumber = currentpage; 
                            //FJConfig.ORDERCURRENTPAGE = pageNumber.ToString(); 失败了还需要存最新的吗，那不会数据丢失？break之后下面的也不会执行
                            log.Info("订单下载ERP执行失败");
                            break;
                        }
                    }
                    #endregion
                }
                else if (i == "2")
                {
                    #region 撤销订单
                    while (iswc == 0)
                    {
                        //从远程取回订单
                        List<Order> comp = tpProxy.GetOrder(branchid, platid, out iswc, out currentpage, pageNumber2, i);
                        if (comp == null)
                        {
                            return;
                        }

                        //2 调用ERP接口将厂家资料写入ERP
                        if (erpProxy.UpdateOrder(comp))
                        {
                            pageNumber2 = currentpage;
                            FJConfig.ORDERCURRENTPAGE2 = pageNumber2.ToString();
                            log.Info("订单下载ERP执行成功");
                        }
                        else
                        {
                            //pageNumber = currentpage;
                            //FJConfig.ORDERCURRENTPAGE = pageNumber.ToString();
                            log.Info("订单下载ERP执行失败");
                            break;
                        }
                    }
                    #endregion
                }
                else if (i == "3")
                {
                    #region 作废订单
                    while (iswc == 0)
                    {
                        //从远程取回订单
                        List<Order> comp = tpProxy.GetOrder(branchid, platid, out iswc, out currentpage, pageNumber3, i);
                        if (comp == null)
                        {
                            return;
                        }

                        //2 调用ERP接口将厂家资料写入ERP
                        if (erpProxy.UpdateOrder(comp))
                        {
                            pageNumber3 = currentpage;
                            FJConfig.ORDERCURRENTPAGE3 = pageNumber3.ToString();
                            log.Info("订单下载ERP执行成功");
                        }
                        else
                        {
                            //pageNumber = currentpage;
                            //FJConfig.ORDERCURRENTPAGE = pageNumber.ToString();
                            log.Info("订单下载ERP执行失败");
                            break;
                        }
                    }
                    #endregion
                }
                SerializeXML(FJConfig, @"Config\FJConfig.xml");

            }
        }
        
        #endregion

        #region 配送单
        //配送单上传
        public void FJPURCHASE()
        {
            if (DateTime.Now.Hour < 5)
                return;
            //Console.WriteLine("service4 executing ......");
            //log.Info("配送单上传开始......");
            //1 调用ERP接口获取配送单信息
            ////获取还未上传的配送单
            //List<DeliveryBill> bill = erpProxy.GetDeliveryBill(branchid, tpid, 20, 0);
            //string ddmxbhs = "";
            ////根据获取的配送单编码获取对应的发票
            //List<DebitNote> note = erpProxy.GetDebitNoteBymxbhs(ddmxbhs);
            //List<DeliveryAndDebitNoteBill> bill = erpProxy.GetDeliveryAndDebitNoteBill(branchid, tpid, 20, 1);
            List<DeliveryAndDebitNoteBill2> bill = erpProxy.GetDeliveryAndDebitNoteBill2(branchid, platid, 20);
            if (tpProxy == null)
            {
                return;
            }
            if (bill == null)
            {
                return;
            }
            if (bill.Count <= 0)
            {
                return;
            }
            //向平台上传
            List<DeliveryBill> bill1 = tpProxy.UpLoadDeliveryAndDebitNoteBill(bill);
            if (bill1 == null)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.WriteBackDeliveryBillState(bill1))
            {
                log.Info("配送单上传回写执行成功");
            }
            else
            {
                log.Info("配送单上传回写执行失败");
            }

        }
        //退货发票上传 
        #region  退货发票上传 

        public void FJInvoice()
        {
            var dateTime = DateTime.Now.Hour;
            if (dateTime < 5) return;
            int zt = 0;
            int num = 200;
            #region 以下是福建接口
            // goos.BZXX = ptypbm + "," + packingid+","+yymc+","+ scqybm;   //需要把BZXX(备注信息)的字段 同步到视图
            //invoice.ggbz = item.GGBZ; //福建视图没有GGBZ(规格包装) 需要在福建视图增加该字段，并把数据同步过来(说明 GGBZ用GGXHSM字段赋值了)
            #endregion
            List<DeliveryAndDebitNoteFj> debitNotes = erpProxy.GetDebitNotePurchase(branchid, platid,num ,zt);
            if (tpProxy == null)
            {
                return;
            }
            if (debitNotes == null)
            {
                return;
            }
            if (debitNotes.Count < 0)
            {
                return;
            }
            List<DebitNote> list = tpProxy.GetDebitNotePurchase(debitNotes);
            if (list == null&&list.Count<0)
            {
                return;
            }
            if (list.Count > 0)
            {
                log.Info("退货发票上传成功....");

            }
            else
            {
                log.Info("退货发票上传失败....");
            }
            if (erpProxy.WriteBackDebitState(list))
            {
                log.Info("配送单上传回写执行成功....");
            }
            else
            {
                log.Info("脱货单上传回写失败.......");
            }
        }




        #endregion
        #endregion

        #region 退货单
        //退货单下载
        public void FJRETURNORDER()
        {
            if (DateTime.Now.Hour < 5)
                return;
            int iswc = 0;
            //DateTime date;
            int currentPage = 1;
            int pageNumber = 1;
            FJConfig = DeserializeXml(@"Config\FJConfig.xml", typeof(FJConfig)) as FJConfig;
            pageNumber = int.Parse(FJConfig.RETURNORDERCURRENTPAGE);
            if (tpProxy == null)
            {
                return;
            }
            while (iswc == 0)
            {
                //从远程取回收货单
                List<ReturnOrder> comp = tpProxy.GetReturnOrder(branchid, platid, out iswc, out currentPage, pageNumber);
                if (comp == null)
                {
                    return;
                }
                //2 调用ERP接口将收货单写入ERP
                if (erpProxy.UploadReturnOrder(comp))
                {
                    //HNConfig.RKORDERLASTTIME = endTime.ToString();
                    //HNConfig.RKORDERCURRENTPAGE = "1";
                    pageNumber = currentPage;
                    FJConfig.RETURNORDERCURRENTPAGE = pageNumber.ToString();
                    log.Info("收货单下载执行成功");
                }
                else
                {
                    pageNumber = currentPage;
                    FJConfig.RETURNORDERCURRENTPAGE = pageNumber.ToString();
                    log.Info("收货单下载执行失败");
                    break;
                }
                //SerializeXML(HNConfig, @"Config\HNConfig.xml");
                SerializeXML(FJConfig, @"Config\FJConfig.xml");
            }
        }

        #endregion

        #region 两票制的票据信息

        //获取平台上已关联的票据信息
        public void FJINVOICEDATAGET()
        {
            if (DateTime.Now.Hour < 5)
                return;
            int iswc = 0;
            int currentpage = 1;
            int pageNumber = 1;
            //Console.WriteLine("service4 executing ......");
            //log.Info("订单下载开始......");
            if (tpProxy == null)
            {
                return;
            }
            //从本地配置文件获取请求参数
            FJConfig = DeserializeXml(@"Config\FJConfig.xml", typeof(FJConfig)) as FJConfig;
            pageNumber = int.Parse(FJConfig.InvoiceDataCURRENTPAGE);

            while (iswc == 0)
            {
                //从远程取回订单
                List<DebitNote> comp = tpProxy.InvoiceDataGet(branchid, platid, out iswc, out currentpage, pageNumber);
                if (comp == null)
                {
                    return;
                }

                //2 调用ERP接口将厂家资料写入ERP
                if (erpProxy.UploadInvoiceData(comp))
                {
                    pageNumber = currentpage;
                    FJConfig.InvoiceDataCURRENTPAGE = pageNumber.ToString();
                    log.Info("订单下载ERP执行成功");
                }
                else
                {
                    pageNumber = currentpage;
                    FJConfig.InvoiceDataCURRENTPAGE = pageNumber.ToString();
                    log.Info("订单下载ERP执行失败");
                    break;
                }
                SerializeXML(FJConfig, @"Config\FJConfig.xml");

            }
        }


        //票据信息上传
        public void FJINVOICEDATAUPLOAD()
        {
            if (DateTime.Now.Hour < 5)
                return;
            //Console.WriteLine("service4 executing ......");
            //log.Info("配送单上传开始......");
            //1 调用ERP接口获取配送单信息
            ////获取还未上传的配送单
            List<DebitNote> bill = erpProxy.GetDebitNote(branchid, platid, 20, 0, 0);
            if (tpProxy == null)
            {
                return;
            }
            if (bill == null)
            {
                return;
            }
            if (bill.Count <= 0)
            {
                return;
            }
            //向平台上传
            List<DebitNote> bill1 = tpProxy.InvoiceDataUpload(bill);
            if (bill1 == null)
            {
                return;
            }
            //3调用ERP接口将返回状态写入ERP
            if (erpProxy.WriteBackDebitState(bill1))
            {
                log.Info("配送单上传回写执行成功");
            }
            else
            {
                log.Info("配送单上传回写执行失败");
            }

        }
        #endregion

        #endregion


        #region 北京

        #region 测试框架用

        //北京test
        public void BJTEST()
        {
            tpProxy.Test(branchid, platid);
            erpProxy.Test(branchid, platid);
        }
        #endregion

        #region 订单下载
        /// <summary>
        /// Dev：付金林 2017/2/16
        /// </summary>
        public void BJORDER()
        {
            int iswc = 0;
            string message;
            List<Goods> goods;
            List<Hospital> hospital;
            if (tpProxy == null)
            {
                return;
            }
            while (iswc == 0)
            {
                List<Order> comp = tpProxy.GetOrder(branchid, platid, out iswc, log, out message, out goods, out hospital);
                if (comp != null && goods != null && hospital != null & comp.Count > 0 && goods.Count > 0 && hospital.Count > 0)
                {
                    BjConfig = DeserializeXml(@"Config\BJConfig.xml", typeof(BJConfig)) as BJConfig;
                    long lastDateTime = long.Parse(BjConfig.lastDateTime);
                    List<Order> list = new List<Order>();
                    for (int i = 0; i < comp.Count; i++)
                    {
                        string time1 = DateTime.Parse(comp[i].JDSJ).ToString("yyyy-MM-dd hh:mm:ss");
                        string time = time1.Replace("-", "").Replace(":", "").Replace(" ", "");
                        long thisTime = long.Parse(time);
                        if (lastDateTime < thisTime)
                        {
                            list.Add(comp[i]);
                        }
                    }
                    if (list.Count < 0 && list == null)
                    {
                        return;
                    }
                    if (erpProxy.UploadOrder(list) /*&& erpProxy.InsertProductInfo(goods)*/&& erpProxy.InsertHospitalInfo(hospital))
                    {
                        List<long> nums = new List<long>();
                        BjConfig.lastDateTime = "";
                        for (int i = 0; i < list.Count; i++)
                        {
                            string time3 = DateTime.Parse(list[i].JDSJ).ToString("yyyy-MM-dd hh:mm:ss");
                            //BjConfig.lastDateTime = time3;
                            nums.Add(long.Parse(time3.Replace("-", "").Replace(":", "").Replace(" ", "")));
                        }
                        long num = nums.Max();
                        BjConfig.lastDateTime = num.ToString().Insert(4, "/").Insert(7, "/").Insert(10, " ").Insert(13, ":").Insert(16, ":");
                        DateTime dt = Convert.ToDateTime(BjConfig.lastDateTime).AddDays(-1);
                        BjConfig.lastDateTime = "";
                        BjConfig.lastDateTime = dt.ToString("yyyy-MM-dd hh: mm:ss");
                        BjConfig.lastDateTime = BjConfig.lastDateTime.Replace("-", "").Replace(":", "").Replace(" ", "");
                        BjConfig.lastDateTime = BjConfig.lastDateTime;
                        SerializeXML(BjConfig, @"Config\BJConfig.xml");
                        log.Info("订单下载,商品插入，医院插入，ERP执行成功...");

                    }
                    else
                    {
                        log.Info("订单下载,商品插入，医院插入的值为空.....");
                        return;
                    }

                }
                else
                {
                    log.Info("订单为空，取不到订单....");
                    return;
                }
            }
        }


        #endregion

        #region 订单配送
        /// <summary>
        /// Dev：付金林 2017/2/20
        /// </summary>
        public void BJPURCHASE()
        {
            //UpLoadDeliveryBill
            //List<DeliveryBill> deBill = tpProxy.UpLoadDeliveryBill()

            List<DeliveryAndDebitNoteBill2> comp1 = erpProxy.GetDeliveryAndDebitNoteBill2(branchid, platid, 500);

            if (comp1 == null)
            {
                return;
            }
            if (comp1.Count < 0)
            {
                return;
            }
            for (int i = 1; i <= comp1.Count / 10 + 1; i++)
            {
                List<DeliveryAndDebitNoteBill2> comp = comp1.Skip((i - 1) * 10).Take(10).ToList();


                if (comp == null)
                {
                    return;
                }
                if (tpProxy == null)
                {
                    return;
                }
                if (comp.Count < 0)
                {
                    return;
                }
                List<DeliveryBill> list = tpProxy.UpLoadDeliveryBill2(comp);
                if (list.Count > 0)
                {
                    log.Info("BJ配送单回传成功！！");
                }
                else
                {
                    log.Info("BJ配送单回传失败！！！");
                    continue;
                }
                //3调用ERP接口将返回状态写入ERP
                if (erpProxy.WriteBackDeliveryBillState(list))
                {
                    log.Info("配送单上传回写执行成功");
                }
                else
                {
                    log.Info("配送单上传回写执行失败");
                }
            }


        }
        #endregion

        #endregion


        #region 上海GPO

        #region 测试框架用

        //上海GPOtest
        public void SHGPOTEST()
        {
            tpProxy.Test(branchid, platid);
            erpProxy.Test(branchid, platid);
        }
        #endregion

        #region 配送单

        //配送单上传
        public void SHGPOPURCHASE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("配送单上传开始......");
            if (tpProxy == null)
            {
                return;
            }
            //1 先找出主单单据编号
            List<string> data = erpProxy.GetGroupPSDH(branchid, platid, 20, 1,"IS_SC_GPO");
            foreach (string psdh in data)
            {
                //根据主单编号获取对应的明细信息
                //1 调用ERP接口获取配送单信息
                List<DeliveryBill> bill = erpProxy.GetDeliveryBillByPSDH(branchid, platid, psdh);

                if (bill == null || bill.Count == 0)
                {
                    continue;
                }
                //向平台上传
                List<DeliveryBill> bill1 = tpProxy.UpLoadDeliveryBill(bill, null);
                if (bill1 == null)
                {
                    continue;
                }
                if (bill1.Count == 0)
                {
                    continue;
                }
                //3调用ERP接口将返回状态写入ERP
                if (erpProxy.WriteBackDeliveryBillState(bill1))
                {
                    log.Info("配送单上传回写执行成功");
                }
                else
                {
                    log.Info("配送单上传回写执行失败");
                }
            }
        }
        
        #endregion

        #region 发票

        //正常发票上传
        public void SHGPODEBITNOTE()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("发票上传开始......");

            if (tpProxy == null)
            {
                return;
            }
            //1 先找出主单单据编号
            List<string> bill = erpProxy.GetGroupFPH(branchid, platid, 20, 0, "IS_SC_GPO",0);
            if (bill == null)
            {
                return;
            }
            foreach (string fph in bill)
            {
                //根据主单编号获取对应的明细信息
                List<DebitNote> note = erpProxy.GetDebitNoteByFPH(branchid, platid, fph);
                if (note == null || note.Count == 0)
                {
                    continue;
                }
                //向平台上传
                List<DebitNote> bill1 = tpProxy.UpLoadDebitNote(note,"0");
                if (bill1 == null)
                {
                    continue;
                }
                if (bill1.Count == 0)
                {
                    continue;
                }
                //3调用ERP接口将返回状态写入ERP 回写成功了
                if (erpProxy.WriteBackDebitState(bill1))
                {
                    log.Info("发票上传回写执行成功");
                }
                else
                {
                    log.Info("发票上传回写执行失败");
                }
            }
        }


        //退货冲红发票上传
        public void SHGPODEBITNOTE1()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("发票上传开始......");

            if (tpProxy == null)
            {
                return;
            }
            //1 先找出主单单据编号
            List<string> bill = erpProxy.GetGroupFPH(branchid, platid, 20, 0, "IS_SC_GPO",1);
            if (bill == null)
            {
                return;
            }
            foreach (string fph in bill)
            {
                //根据主单编号获取对应的明细信息
                List<DebitNote> note = erpProxy.GetDebitNoteByFPH(branchid, platid, fph);
                if (note == null || note.Count == 0)
                {
                    continue;
                }
                //向平台上传
                List<DebitNote> bill1 = tpProxy.UpLoadDebitNote(note,"1");
                if (bill1 == null)
                {
                    continue;
                }
                if (bill1.Count == 0)
                {
                    continue;
                }
                //3调用ERP接口将返回状态写入ERP 回写成功了
                if (erpProxy.WriteBackDebitState(bill1))
                {
                    log.Info("发票上传回写执行成功");
                }
                else
                {
                    log.Info("发票上传回写执行失败");
                }
            }
        }

        //调价补差发票上传
        public void SHGPODEBITNOTEXSB()
        {
            //Console.WriteLine("service4 executing ......");
            log.Info("调价补差发票单上传开始......");

            if (tpProxy == null)
            {
                return;
            }
            //1 先找出主单单据编号
            List<string> bill = erpProxy.GetGroupFPHXSB(branchid, platid, 20, 0);
            if (bill == null)
            {
                return;
            }
            foreach (string fph in bill)
            {
                //根据主单编号获取对应的明细信息
                List<DebitNoteXSB> note = erpProxy.GetDebitNoteXSBByFPH(branchid, platid, fph);
                if (note == null || note.Count == 0)
                {
                    continue;
                }
                //向平台上传
                List<DebitNoteXSB> bill1 = tpProxy.UpLoadDebitNoteXSB(note);
                if (bill1 == null)
                {
                    continue;
                }
                if (bill1.Count == 0)
                {
                    continue;
                }
                //3调用ERP接口将返回状态写入ERP 回写成功了
                if (erpProxy.WriteBackDebitXSBState(bill1))
                {
                    log.Info("调价补差发票上传回写执行成功");
                }
                else
                {
                    log.Info("调价补差发票上传回写执行失败");
                }
            }
        }

        #endregion

        #region 货款支付

        //货款支付信息下载
        public void SHGPOPAYMENTDOWNLOAD()
        {
            int iswc = 0;

            log.Info("货款支付单下载开始......");
            if (tpProxy == null)
            {
                return;
            }
            //从本地配置文件获取请求参数
            SHGPOConfig = DeserializeXml(@"Config\SHGPOConfig.xml", typeof(SHGPOConfig)) as SHGPOConfig;
            string a = SHGPOConfig.lastpaymentbm.Replace("\"", "");
            string returnlastpaymentbm = "";
            //先获取所有的GPO医院信息
            //SELECT * FROM base_plat_yyxx WHERE isgpo=1 
            List<Hospital> hoslist = erpProxy.GetHospitalInfoBy(branchid, platid, "isgpo", "1");
            foreach (Hospital hos in hoslist)
            {
                while (iswc == 0)
                {
                    if (returnlastpaymentbm != "")
                        a = returnlastpaymentbm;
                    //从远程取回订单
                    List<Payment> comp = tpProxy.GetPaymentOrder(branchid, platid, a, out iswc, out returnlastpaymentbm, hos.YYBM);
                    if (comp == null)
                    {
                        //return;
                        continue;
                    }
                    if (comp.Count == 0)
                    {
                        //return;
                        continue;
                    }
                    //2 调用ERP接口将厂家资料写入ERP
                    if (erpProxy.UploadPaymentOrder(comp))
                    {
                        SHGPOConfig.lastpaymentbm = returnlastpaymentbm;
                        log.Info("货款支付单下载ERP执行成功");
                    }
                    else
                    {
                        log.Info("货款支付单下载执行失败");
                        //return;
                        continue;
                    }
                    SerializeXML(SHGPOConfig, @"Config\SHGPOConfig.xml");
                }
            }
        }

        //货款发票销账信息上传
        public void SHGPOPAYMENTUPLOAD()
        {
            log.Info("货款发票销账上传开始......");
            //1 先找出主单单据编号
            List<string> data = erpProxy.GetGroupZFDBH(branchid, platid, 20, 2);
            foreach (string zfdbh in data)
            {
                //根据主单编号获取对应的明细信息
                #region 
                //1 调用ERP接口获取订单响应信息
                List<Payment> bill = erpProxy.GetPaymentOrder(branchid, platid, zfdbh);

                if (tpProxy == null)
                {
                    return;
                }
                if (bill == null || bill.Count == 0)
                {
                    return;
                }
                //向平台上传
                List<Payment> bill1 = tpProxy.UpLoadPaymentOrder(bill);
                if (bill1 == null)
                {
                    return;
                }
                if (bill1.Count == 0)
                {
                    return;
                }
                //3调用ERP接口将返回状态写入ERP  回写成功
                if (erpProxy.UpdatePaymentOrder(bill1))
                {
                    log.Info("货款发票销账回写执行成功");
                }
                else
                {
                    log.Info("货款发票销账回写执行失败");
                }
                #endregion
            }
        }
        #endregion

        #endregion


        public BJConfig BjConfig { get; set; }
        public HNConfig HNConfig { get; set; }
        public SHQXConfig SHQXConfig { get; set; }
        public SHGPOConfig SHGPOConfig { get; set; }
        public FJConfig FJConfig { get; set; }
        public void SerializeXML(object objectToConvert, string path)
        {
            if (objectToConvert != null)
            {
                Type t = objectToConvert.GetType();

                XmlSerializer ser = new XmlSerializer(t);

                using (StreamWriter writer = new StreamWriter(path))
                {
                    ser.Serialize(writer, objectToConvert);
                    writer.Close();
                }
            }
        }

        public object DeserializeXml(string fileName, Type tp)
        {
            if (!File.Exists(fileName)) return null;
            XmlSerializer ser = new XmlSerializer(tp);
            object o = null;
            try
            {
                using (StreamReader read = new StreamReader(fileName))
                {
                    o = ser.Deserialize(read);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return o;
        }
    }
}

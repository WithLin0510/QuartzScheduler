using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using log4net;
using TPI.Common;
using TPI.Common.Busi;
using System.Net;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;

namespace TPI.BLL
{
    /// <summary>
    /// 对erp实现实现的接口。 
    /// </summary>
    public class ErpProxy : IErpProxy, ILogManager
    {

        private ILog log;
        /// <summary>
        /// 日志引用
        /// </summary>
        /// <param name="logger"></param>
        public void SetLog(ILog logger)
        {
            log = logger;
        }

        private string ERPAPi;
        /// <summary>
        /// API请求地址引用
        /// </summary>
        /// <param name="url"></param>
        public void SetERPUrl(string url)
        {
            ERPAPi = url;
        }

        private string CustomErpAPi;
        /// <summary>
        /// ERP自定义API请求地址引用
        /// </summary>
        /// <param name="url"></param>
        public void SetCustomERPUrl(string url)
        {
            CustomErpAPi = url;
        }

        /// <summary>
        /// HTTP POST公用方法
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="param">请求内容</param>
        /// <returns></returns>
        private static string Post(string url, string param)
        {
            try
            {
                System.Net.WebRequest myHttpWebRequest = System.Net.WebRequest.Create(url);
                myHttpWebRequest.Method = "post";
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] byte1 = encoding.GetBytes(param);
                myHttpWebRequest.ContentType = "application/json;charset=UTF-8";
                myHttpWebRequest.ContentLength = byte1.Length;
                System.IO.Stream newStream = myHttpWebRequest.GetRequestStream();
                newStream.Write(byte1, 0, byte1.Length);
                newStream.Close();
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                string lcHtml = string.Empty;
                Encoding enc = Encoding.GetEncoding("UTF-8");
                System.IO.Stream stream = response.GetResponseStream();
                System.IO.StreamReader streamReader = new System.IO.StreamReader(stream, enc);
                lcHtml = streamReader.ReadToEnd();
                return lcHtml;
            }
            catch (WebException ex)
            {
                string message = string.Format("{0}{1}", ex.Message, Environment.NewLine);//先将WebException的异常赋值

                HttpWebResponse errResp = ex.Response as HttpWebResponse;
                string readerMessage = "";
                if (errResp != null)
                {
                    using (Stream respStream = errResp.GetResponseStream())
                    {
                        if (respStream != null)
                        {
                            StreamReader reader = new StreamReader(respStream);
                            readerMessage = reader.ReadToEnd();

                            //if (errResp.StatusCode == HttpStatusCode.BadRequest)//如果返回 400 错误，读取服务端自定义的异常信息
                            //{
                            //    ResponseTokenError error = JsonConvert.DeserializeObject<ResponseTokenError>(readerMessage);

                            //    throw new WebException(error.Error_Description);
                            //}
                            //else
                            //{
                            //    message += readerMessage;
                            //}
                        }
                    }
                }

                throw new WebException(string.Format("请求地址：{0}{1}{2}", url, Environment.NewLine, readerMessage));
            }
        }

        /// <summary>
        /// HTTP GET公用方法
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        private static string Get(string url)
        {
            System.Net.WebRequest myHttpWebRequest = System.Net.WebRequest.Create(url);
            myHttpWebRequest.Method = "get";
            try
            {
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                string lcHtml = string.Empty;
                Encoding enc = Encoding.GetEncoding("UTF-8");
                System.IO.Stream stream = response.GetResponseStream();
                System.IO.StreamReader streamReader = new System.IO.StreamReader(stream, enc);
                lcHtml = streamReader.ReadToEnd();
                return lcHtml;
            }
            catch (WebException ex)
            {
                string message = string.Format("{0}{1}", ex.Message, Environment.NewLine);//先将WebException的异常赋值

                HttpWebResponse errResp = ex.Response as HttpWebResponse;
                string readerMessage = "";
                if (errResp != null)
                {
                    using (Stream respStream = errResp.GetResponseStream())
                    {
                        if (respStream != null)
                        {
                            StreamReader reader = new StreamReader(respStream);
                            readerMessage = reader.ReadToEnd();

                            //if (errResp.StatusCode == HttpStatusCode.BadRequest)//如果返回 400 错误，读取服务端自定义的异常信息
                            //{
                            //    ResponseTokenError error = JsonConvert.DeserializeObject<ResponseTokenError>(readerMessage);

                            //    throw new WebException(error.Error_Description);
                            //}
                            //else
                            //{
                            //    message += readerMessage;
                            //}
                        }
                    }
                }

                throw new WebException(string.Format("请求地址：{0}{1}{2}", url, Environment.NewLine, readerMessage));
            }
        }

        /// <summary>
        /// 自定义API
        /// </summary>
        class DefinedAPI
        {
            /// <summary>
            /// SQL脚本，直接查询的SQL语句
            /// </summary>
            public string SqlScript { get; set; }

            /// <summary>
            /// 参数列表
            /// </summary>
            public List<ParamInfo> ParamList { get; set; }

            /// <summary>
            /// 是否分页
            /// </summary>
            public bool IsPaged { get; set; }

            /// <summary>
            /// 每页显示条数
            /// </summary>
            public int PageSize { get; set; }

            /// <summary>
            /// 当前页数
            /// </summary>
            public int PagePosition { get; set; }

        }
        /// <summary>
        /// Dev:付金林  2017/3/29 
        /// </summary>
        class ParamInfo
        {
            public ParamInfo(string paramName, string paramType, string paramValue)
            {
                this.ParamName = paramName;
                this.ParamType = paramType;
                //this.ParamValue = ParamValue;
                this.ParamValue = paramValue;
            }
            /// <summary>
            /// 参数名
            /// </summary>
            public string ParamName { get; set; }

            /// <summary>
            /// 参数类型
            /// </summary>
            public string ParamType { get; set; }

            /// <summary>
            /// 参数值
            /// </summary>
            public string ParamValue { get; set; }
        }
        /// <summary>
        /// Dev:付金林 2017/2/29
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class ReturnMsg<T>
        {
            /// <summary>
            /// 结果集
            /// </summary>
            public Table1<T> ResultSet { get; set; }

            /// <summary>
            /// 返回的条目数
            /// </summary>
            public string RecordCount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string ResultByte { get; set; }
        }
        /// <summary>
        /// Dev:付金林 2017/2/29
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class Table1<T>
        {
            /// <summary>
            /// 
            /// </summary>
            public List<T> Table { get; set; }
        }
        /// <summary>
        /// Dev：付金林 针对Cout（*） 函数 返回的数字序列化 2017/05/08
        /// </summary>
        class Count
        {
            public string count { get; set; }
        }
        /// <summary>
        /// Dev：付金林 2017/3/29 ErpApi返回的数据
        /// </summary>
        /// <param name="apiData">DefinedAPI对象</param>
        private string ApiRetrunData(DefinedAPI apiData)
        {
            //第一步将列表生成json 
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(apiData);
            //第二步通过ERPAPI接口写入ERP
            string a = Post(CustomErpAPi, json);
            return a;
        }
        #region 测试框架用
        public void Test(string branchID, string TPId)
        {
            log.Info("erptest:erpurl-" + ERPAPi);
        }
        #endregion

        #region 资料

        #region 商品
        /// <summary>
        /// 向 erp 上传商品资料信息
        /// </summary>
        /// <param name="goods">商品资料实体集合</param>
        /// <returns></returns>
        public bool InsertProductInfo(List<Goods> goods)
        {
            //分页更新

            try
            {
                if (goods == null)
                    return false;
              
                string url = ERPAPi + "AddDrugBase";
                string msg = "";
                for (int i = 1; i <= goods.Count / 200 + 1; i++)
                {
                    List<Goods> list1 = goods.Skip((i - 1) * 200).Take(200).ToList();
                    //将商品资料转换成JSON格式
                    string json1 = Newtonsoft.Json.JsonConvert.SerializeObject(list1);

                    //通过ERPAPI接口写入ERP
                    if (Post(url, json1) != "true")
                        msg += "error";
                    
                    if (goods.Count == 200 * i)
                    {
                        break;
                    }
                }
                if (msg == "")
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
                log.Error("上传商品资料：异常报错-" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 根据商品ID获取商品信息后插入erp
        /// </summary>
        /// <param name="goods"></param>
        /// <returns></returns>
        public bool InsertProductInfobyid(List<Goods> goods, string ERPAPi1)
        {
            try
            {
                if (goods == null)
                    return false;
                string url = ERPAPi1 + "AddDrugBase";

                //1 将商品资料转换成JSON格式
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(goods);
                //2 通过ERPAPI接口写入ERP
                if (Post(url, json) == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("上传商品资料：异常报错-" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 获取erp最大一笔添加日期 max(addtime)
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <returns></returns>
        public string IsExistProductInfo(string branchid, string platid)
        {
            try
            {
                string url = ERPAPi + "GetDrugMaxDate" + "?branchid=" + branchid + "&platid=" + platid;
                string data = Get(url);
                string date = data.Replace("\"", "");
                return date;
            }
            catch (Exception ex)
            {
                log.Error("获取商品资料最大时间：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据传递的时间获取时间(addtime)大于传递的这个时间的数量
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="date">开始时间</param>
        /// <returns></returns>
        public int GetProductInfoCount(string branchid, string platid, string date)
        {
            try
            {
                string url = ERPAPi + "GetDrugCount" + "?branchid=" + branchid + "&platid=" + platid + "&lastdate=" + date;
                string data = Get(url);
                return int.Parse(data);
            }
            catch (Exception ex)
            {
                log.Error("获取商品资料条目数：异常报错-" + ex.ToString());
                return 0;
            }
        }
        /// <summary>
        /// 获取所有商品的总和 Dev：付金林 2017/05/08
        /// </summary>
        /// <param name="branchid">Feb公司标志</param>
        /// <param name="platid">平台Id</param>
        /// <param name="date">先占位以后可能用到</param>
        /// <returns></returns>
        public int GetProductInfoAllCount(string branchid, string platid, string date)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select COUNT(*) as count from DRUG_BASE_Inf where branchid=:BranchId and platid=:PlatId";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            //list.Add(new ParamInfo("Date", "String",date.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            ReturnMsg<Count> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<Count>>(ApiRetrunData(api));
            var count = int.Parse(order.ResultSet.Table[0].count);
            return count;

        }
        #endregion

        #region 医院
        /// <summary>
        /// 向 erp 上传医院信息
        /// </summary>
        /// <param name="order">医院资料实体集合</param>
        /// <returns></returns>
        public bool InsertHospitalInfo(List<Hospital> order)
        {
            try
            {
                if (order == null)
                    return false;
                string url = ERPAPi + "AddYYXX";
                //1 将商品资料转换成JSON格式
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(order);
                //2 通过ERPAPI接口写入ERP
                if (Post(url, json) == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("上传医院资料：异常报错-" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 获取医院信息的总条目数
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <returns></returns>
        public int GetHospitalInfoCount(string branchid, string platid)
        {
            try
            {
                string url = ERPAPi + "GetYYXXCount" + "?branchid=" + branchid + "&platid=" + platid;
                string data = Get(url);
                return int.Parse(data);
            }
            catch (Exception ex)
            {
                log.Error("获取医院资料条目数：异常报错-" + ex.ToString());
                return -1;
            }
            #region 自定义查询
            //DefinedAPI api = new DefinedAPI();
            //List<ParamInfo> list = new List<ParamInfo>();
            //api.SqlScript = "SELECT count(*) as count FROM hnqerp.base_plat_yyxx WHERE branchid=:BranchId and platid=:PlatId  ";
            //list.Add(new ParamInfo("BranchId", "String", branchid));
            //list.Add(new ParamInfo("PlatId", "String", platid));
            //api.ParamList = list;
            //api.IsPaged = false;
            //try
            //{
            //    ReturnMsg<Count> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<Count>>(ApiRetrunData(api));
            //    if (order.ResultSet.Table.Count <= 0) return 0;
            //    Count table = new Count();
            //    table = order.ResultSet.Table[0];
            //    return int.Parse( table.count);
            //}
            //catch (Exception ex)
            //{
            //    log.Error("获取医院资料条目数：异常报错-" + ex.ToString());
            //    return 0;
            //}
            #endregion
        }

        /// <summary>
        /// 获取当前条件下的医院数据
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="zt">要获取的状态</param>
        /// <param name="num">要获取的条目数</param>
        /// <returns></returns>
        public List<Hospital> GetHospitalInfo(string branchid, string platid, int zt, int num)
        {
            try
            {
                string url = ERPAPi + "GetPlatYYXX" + "?branchid=" + branchid + "&platid=" + platid + "&num=" + num + "&zt=" + zt;
                string data = Get(url);
                if (data.Replace("[", "").Replace("]", "") == "")
                    return null;
                //第二步将JSON返回值序列化成实体类
                List<Hospital> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Hospital>>(data);
                return order;
            }
            catch (Exception ex)
            {
                log.Error("获取待阅读的订单：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 修改医院信息的是否提取配送点的状态
        /// </summary>
        /// <param name="hospitals">医院资料实体集合</param>
        /// <returns></returns>
        public bool UpdateHospital(List<Hospital> hospitals)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 自定义-----------根据条件获取医院信息  20170510 yhyy加 上海GPO平台用到
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="param">字段名</param>
        /// <param name="value">字段取值</param>
        /// <returns></returns>
        public List<Hospital> GetHospitalInfoBy(string branchid, string platid, string param, string value)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "SELECT * FROM base_plat_yyxx WHERE branchid=:BranchId and platid=:PlatId and  " + param + "=:param ";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("param", "String", value));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<Hospital> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<Hospital>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<Hospital> table = new List<Hospital>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    Hospital deliveryBill = order.ResultSet.Table[i];
                    table.Add(deliveryBill);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }
        #endregion

        #region 医院配送点
        /// <summary>
        /// 向 erp 上传医院配送点信息
        /// </summary>
        /// <param name="order">医院配送点实体集合</param>
        /// <returns></returns>
        public bool InsertHospitalPSDInfo(List<HospitalPSD> order)
        {
            try
            {
                if (order == null)
                    return false;
                string url = ERPAPi + "AddCustomerBaseInfo";
                //1 将资料转换成JSON格式
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(order);
                //2 通过ERPAPI接口写入ERP
                if (Post(url, json) == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("上传医院配送点：异常报错-" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 获取erp最大一笔添加日期 max(addtime)
        /// </summary>
        /// <param name="branchid"></param>
        /// <param name="platid"></param>
        /// <returns></returns>
        public string IsExistHospitalPSDInfo(string branchid, string platid)
        {
            try
            {
                string url = ERPAPi + "GetCustomerMaxDate" + "?branchid=" + branchid + "&platid=" + platid;
                string data = Get(url);
                string date = data.Replace("\"", "");
                return date;
            }
            catch (Exception ex)
            {
                log.Error("获取医院配送点最大时间：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据传递的时间获取配送点信息时间(addtime)大于传递的这个时间的数量
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="date">开始时间</param>
        /// <returns></returns>
        public int GetHospitalPSDInfoCount(string branchid, string platid, string date)
        {
            try
            {
                string url = ERPAPi + "GetCustomerCount" + "?branchid=" + branchid + "&platid=" + platid + "&lastdate=" + date;
                string data = Get(url);
                return int.Parse(data);
            }
            catch (Exception ex)
            {
                log.Error("获取医院配送点条目数：异常报错-" + ex.ToString());
                return 0;
            }
        }
        #endregion

        #region 生产厂家

        /// <summary>
        /// 向 erp 上传生产厂家信息
        /// </summary>
        /// <param name="companys">生产厂家实体集合</param>
        /// <returns></returns>
        public bool InsertCompanyInfo(List<Companys> companys)
        {
            try
            {
                if (companys == null)
                    return false;
                string url = ERPAPi + "AddCompanyBaseInfo";
                //第一步将列表生成json 
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(companys);
                //第二步通过ERPAPI接口写入ERP
                if (Post(url, json) == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("上传厂家资料：异常报错-" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 获取erp最大一笔添加日期 max(addtime)
        /// </summary>
        /// <param name="branchid"></param>
        /// <param name="platid"></param>
        /// <returns></returns>
        public string IsExistCompanyInfo(string branchid, string platid)
        {
            try
            {
                string url = ERPAPi + "GetCompanyMaxDate" + "?branchid=" + branchid + "&platid=" + platid;
                string data = Get(url);
                string date = data.Replace("\"", "");
                return date;
            }
            catch (Exception ex)
            {
                log.Error("获取生产厂家最大时间：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据传递的时间获取厂家信息时间(addtime)大于传递的这个时间的数量
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="date">开始时间</param>
        /// <returns></returns>
        public int GetCompanyInfoCount(string branchid, string platid, string date)
        {
            try
            {
                string url = ERPAPi + "GetCompanyCount" + "?branchid=" + branchid + "&platid=" + platid + "&lastdate=" + date;
                string data = Get(url);
                return int.Parse(data);
            }
            catch (Exception ex)
            {
                log.Error("获取厂家条目数：异常报错-" + ex.ToString());
                return 0;
            }
        }
        #endregion

        #endregion

        #region 订单

        /// <summary>
        /// 向 erp 上传订单
        /// </summary>
        /// <param name="order">订单实体集合</param>
        /// <returns></returns>
        public bool UploadOrder(List<Order> order)
        {
            //加入分页功能
            try
            {
                if (order == null)
                    return false;
                string url = ERPAPi + "AddDrugBaseBillDD";
                string msg = "";
                for (int i = 1; i <= order.Count / 20 + 1; i++)
                {
                    List<Order> list1 = order.Skip((i - 1) * 20).Take(20).ToList();
                    if (list1.Count != 0 && list1 != null)
                    {

                        //将商品资料转换成JSON格式
                        string json1 = Newtonsoft.Json.JsonConvert.SerializeObject(list1);

                        //通过ERPAPI接口写入ERP
                        if (Post(url, json1) != "true")
                            msg += "error";
                    }
                }
                if (msg == "")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString() + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 获取当前订单表中最大一笔订单提交日期DDTJRQ
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <returns></returns>
        public string GetLastDDTJRQ(string branchid, string platid)
        {
            try
            {
                string url = ERPAPi + "GetBillDDMaxDate" + "?branchid=" + branchid + "&platid=" + platid;
                string data = Get(url);
                return data;
            }
            catch (Exception ex)
            {
                log.Error("获取订单表最大提交时间：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 从ERP获取待阅读的订单
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <returns></returns>
        public List<Order> GetReadOrderInfo(string branchid, string platid, int num)
        {
            try
            {
                string url = ERPAPi + "GetBillDDReadNum" + "?branchid=" + branchid + "&platid=" + platid + "&num=" + num;
                string data = Get(url);
                //第二步将JSON返回值序列化成实体类
                List<Order> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Order>>(data);
                return order;
            }
            catch (Exception ex)
            {
                log.Error("获取待阅读的订单：异常报错-" + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 从ERP获取待响应的订单 
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="ddresponse">ddresponse要获取的响应状态（响应或者拒绝响应）</param>
        /// <returns></returns>
        public List<Order> GetResponseOrderInfo(string branchid, string platid, int num, int ddresponse)
        {
            try
            {
                string url = ERPAPi + "GetBillDDResponseNum" + "?branchid=" + branchid + "&platid=" + platid + "&num=" + num + "&zt=" + ddresponse;
                string data = Get(url);
                //第二步将JSON返回值序列化成实体类
                List<Order> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Order>>(data);
                return order;
            }
            catch (Exception ex)
            {
                log.Error("获取待响应的订单：异常报错-" + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 从ERP获取待响应的订单
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="ddresponse">ddresponse要获取的响应状态（响应或者拒绝响应）</param>
        /// <param name="issc">是否上传</param>
        /// <returns></returns>
        public List<Order> GetResponseOrderInfo(string branchid, string platid, int num, int ddresponse, int issc)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select  * from  drug_base_bill_dd where is_sc = :issc " +
               " AND ddresponse = :ddresponse and branchid = :BranchId and PLATID = :PlatId  and  ROWNUM <= :Num";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("issc", "Int", issc.ToString()));
            list.Add(new ParamInfo("ddresponse", "Int", ddresponse.ToString()));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<Order> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<Order>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<Order> table = new List<Order>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    Order deliveryBill = order.ResultSet.Table[i];
                    table.Add(deliveryBill);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 从ERP获取待上传的代填订单
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="ddcllx">订单处理类型 0-初始 1-确认 2-作废</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="ddresponse">要获取的响应状态</param>
        /// <returns></returns>
        public List<Order> GetDTOrderInfo(string branchid, string platid, int ddcllx, int num, int ddresponse)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "SELECT a.* from drug_base_bill_dd a where YYJHDH in (select  YYJHDH from(select  YYJHDH from " +
               " drug_base_bill_dd where is_sc = 0 and ddcllx = :ddcllx and ddtjfs = 2 AND ddresponse = :ddresponse " +
               "and branchid = :BranchId and PLATID = :PlatId group by YYJHDH) WHERE  ROWNUM <= :Num)";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("ddcllx", "Int", ddcllx.ToString()));
            list.Add(new ParamInfo("ddresponse", "Int", ddresponse.ToString()));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<Order> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<Order>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<Order> table = new List<Order>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    Order deliveryBill = order.ResultSet.Table[i];
                    table.Add(deliveryBill);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 从ERP获取待确认的代填订单
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="ddcllx">订单处理类型 0-初始 1-确认 2-作废</param>
        /// <returns></returns>
        public List<Order> GetDTOrderQueRenInfo(string branchid, string platid, int ddcllx, int num, int ddresponse)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from ( select ddbh, YYBM, PSDBM, DDLX, SPSL, JLS from drug_base_bill_dd " +
                "where is_sc = 0 and ddcllx = :ddcllx and ddtjfs = 2 and ddresponse=:ddresponse  and branchid = :BranchId and PLATID = :PlatId " +
                "group by ddbh,YYBM,PSDBM,DDLX,SPSL, JLS ) WHERE  ROWNUM <= :Num";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("ddcllx", "Int", ddcllx.ToString()));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            list.Add(new ParamInfo("ddresponse", "Int", ddresponse.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<Order> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<Order>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<Order> table = new List<Order>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    Order deliveryBill = order.ResultSet.Table[i];
                    table.Add(deliveryBill);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }
        public List<Order> GetDTOrderQueRenInfo(string branchid, string platid, int ddcllx, string DDBH)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from drug_base_bill_dd where platid=:PlatId and  branchid=:BranchId and ddcllx=:ddcllx and DDBH in  (select * from table (split(:DDBH, ',')))";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("ddcllx", "Int", ddcllx.ToString()));
            list.Add(new ParamInfo("DDBH", "String", DDBH.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ApiRetrunData(api);
                ReturnMsg<Order> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<Order>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<Order> table = new List<Order>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    Order deliveryBill = order.ResultSet.Table[i];
                    table.Add(deliveryBill);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 回写订单状态
        /// </summary>
        /// <param name="order">订单实体集合</param>
        /// <returns></returns>
        public bool UpdateOrder(List<Order> order)
        {
            try
            {
                string url = ERPAPi + "UpdateDrugBaseBillDD";
                //第一步将列表生成json 
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(order);
                //第二步调用ESB将订单信息上传到ERP
                if (Post(url, json) == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("回写订单状态：异常报错-" + ex.ToString());
                return false;
            }
        }
        #endregion

        #region 退货单


        /// <summary>
        /// 向 erp 上传退货单
        /// </summary>
        /// <param name="order">退货单实体集合</param>
        /// <returns></returns>
        public bool UploadReturnOrder(List<ReturnOrder> order)
        {
            try
            {
                string url = ERPAPi + "AddDrugBaseBillTH";
                //第一步将列表生成json 
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(order);
                //第二步通过ERPAPI接口写入ERP
                if (Post(url, json) == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("上传退货单信息：异常报错-" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 获取当前退货单表中最大一笔订单提交日期DDTJRQ
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <returns></returns>
        public string GetLastTDTJRQ(string branchid, string platid)
        {
            try
            {
                string url = ERPAPi + "GetBillTHMaxDate" + "?branchid=" + branchid + "&platid=" + platid;
                string data = Get(url);
                return data;
            }
            catch (Exception ex)
            {
                log.Error("获取退货单表最大提交时间：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 从ERP获取待响应的退货单
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="tdresponse">要获取的响应状态（响应或者拒绝响应）</param>
        /// <returns></returns>
        public List<ReturnOrder> GetResponseReturnOrderInfo(string branchid, string platid, int num, int tdresponse)
        {
            try
            {
                string url = ERPAPi + "GetBillTHResponseNum" + "?branchid=" + branchid + "&platid=" + platid + "&num=" + num + "&zt=" + tdresponse;
                string data = Get(url);
                //第二步将JSON返回值序列化成实体类
                List<ReturnOrder> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ReturnOrder>>(data);
                return order;
            }
            catch (Exception ex)
            {
                log.Error("获取待响应的退货单：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 从ERP获取待响应的退货单
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="tdresponse">要获取的响应状态（响应或者拒绝响应）</param>
        /// <returns></returns>
        public List<ReturnOrder> GetResponseReturnOrderInfo(string branchid, string platid, int num, int tdresponse, int issc)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select  * from  drug_base_bill_th where is_sc = :issc " +
               " AND tdresponse = :tdresponse and branchid = :BranchId and PLATID = :PlatId  and  ROWNUM <= :Num";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("issc", "Int", issc.ToString()));
            list.Add(new ParamInfo("tdresponse", "Int", tdresponse.ToString()));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<ReturnOrder> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<ReturnOrder>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<ReturnOrder> table = new List<ReturnOrder>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    ReturnOrder deliveryBill = order.ResultSet.Table[i];
                    table.Add(deliveryBill);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 从ERP获取待上传的代填退货单
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="ddcllx">订单处理类型 0-初始 1-确认 2-作废</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="tdresponse">要获取的响应状态</param>
        /// <returns></returns>
        public List<ReturnOrder> GetDTReturnOrderInfo(string branchid, string platid, int ddcllx, int num, int tdresponse)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "SELECT a.* from drug_base_bill_th a where JSSJ in (select  JSSJ from(select  JSSJ from " +
           " drug_base_bill_th where is_sc = 0 and ddcllx = :ddcllx and THDTJFS = 2 AND tdresponse = :tdresponse " +
           "and branchid = :BranchId and PLATID = :PlatId group by JSSJ) WHERE ROWNUM <= :Num)";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("ddcllx", "Int", ddcllx.ToString()));
            list.Add(new ParamInfo("tdresponse", "Int", tdresponse.ToString()));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<ReturnOrder> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<ReturnOrder>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<ReturnOrder> table = new List<ReturnOrder>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    ReturnOrder deliveryBill = order.ResultSet.Table[i];
                    table.Add(deliveryBill);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 从ERP获取待确认的代填退货单
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="ddcllx">订单处理类型 0-初始 1-确认 2-作废</param>
        /// <returns></returns>
        public List<ReturnOrder> GetDTReturnOrderQueRenInfo(string branchid, string platid, int ddcllx, int num, int tdresponse)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from ( select  YYBM, PSDBM,  DETAILCOUNT, THDBH from drug_base_bill_th " +
                "where is_sc = 0 and ddcllx = :ddcllx and THDTJFS = 2 and tdresponse=:tdresponse  and branchid = :BranchId and PLATID = :PlatId " +
                "group by YYBM,PSDBM,DETAILCOUNT,THDBH ) WHERE  ROWNUM <= :Num";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("ddcllx", "Int", ddcllx.ToString()));
            list.Add(new ParamInfo("tdresponse", "Int", tdresponse.ToString()));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<ReturnOrder> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<ReturnOrder>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<ReturnOrder> table = new List<ReturnOrder>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    ReturnOrder deliveryBill = order.ResultSet.Table[i];
                    table.Add(deliveryBill);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 回写退货单状态
        /// </summary>
        /// <param name="order">退货单实体集合</param>
        /// <returns></returns>
        public bool UpdateReturnOrder(List<ReturnOrder> order)
        {
            try
            {
                string url = ERPAPi + "UpdateDrugBaseBillTH";
                //第一步将列表生成json 
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(order);
                //第二步调用ESB将订单信息上传到ERP
                if (Post(url, json) == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("回写退货单状态：异常报错-" + ex.ToString());
                return false;
            }
        }
        /// <summary>
        /// 根据订单编号获取 表的属性 从而回写数据库 Dev：付金林 2017/3/17
        /// </summary>
        /// <param name="branchid">分公司标志</param>
        /// <param name="platid">平台id</param>
        /// <param name="ddbhs">订单编号</param>
        /// <param name="ddcllx">订单处理类型 0-初始 1-确认 2-作废</param>
        /// <returns></returns>
        public List<ReturnOrder> GetAllReturnOrderProperty(string branchid, string platid, string thdbhs, int ddcllx)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from drug_base_bill_th where platid=:PlatId and  branchid=:BranchId and ddcllx=:ddcllx and THDBH in (select * from table (split(:THDBH, ',')))";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("ddcllx", "Int", ddcllx.ToString()));
            list.Add(new ParamInfo("THDBH", "String", thdbhs.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<ReturnOrder> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<ReturnOrder>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<ReturnOrder> table = new List<ReturnOrder>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    ReturnOrder deliveryBill = order.ResultSet.Table[i];
                    table.Add(deliveryBill);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        #endregion


        #region 配送信息

        /// <summary>
        /// 获取配送单信息
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="zt">要获取的状态</param>
        /// <returns></returns>
        public List<DeliveryBill> GetDeliveryBill(string branchid, string platid, int num, int zt)
        {
            try
            {
                string url = ERPAPi + "GetDrugBillPSList" + "?branchid=" + branchid + "&platid=" + platid + "&num=" + num + "&zt=" + zt;
                string data = Get(url);
                //data = data.Replace("\\", "");
                //data = data.Substring(1, data.Length - 2);
                //第二步将JSON返回值序列化成实体类
                List<DeliveryBill> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DeliveryBill>>(data);
                return order;
            }
            catch (Exception ex)
            {
                log.Error("获取待上传的配送单：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 获取还未下载医院收货信息的配送单
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <returns></returns>
        public List<DeliveryBill> GetDeliveryBillNotTQ(string branchid, string platid, int num)
        {
            try
            {
                string url = ERPAPi + "GetDrugBillPSNotRKTQ" + "?branchid=" + branchid + "&platid=" + platid + "&num=" + num;
                string data = Get(url);
                //第二步将JSON返回值序列化成实体类
                List<DeliveryBill> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DeliveryBill>>(data);
                return order;
            }
            catch (Exception ex)
            {
                log.Error("获取还未下载医院收货信息的配送单：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 获取还未上传的配送单和发票的视图（ddmxbh,psl,ckph,yxrq,fph）
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <returns></returns>
        public List<DeliveryAndDebitNoteBill1> GetDeliveryAndDebitNoteBill(string branchid, string platid, int num)
        {
            try
            {
                string url = ERPAPi + "GetViewPS" + "?branchid=" + branchid + "&platid=" + platid + "&num=" + num;
                string data = Get(url);

                data = data.Replace("\\", "");
                data = data.Substring(1, data.Length - 2);
                List<DeliveryAndDebitNoteBill1> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DeliveryAndDebitNoteBill1>>(data);
                return order;
            }
            catch (Exception ex)
            {
                log.Error("获取含有发票信息的配送单：异常报错-" + ex.ToString());
                return null;
            }
        }

        public List<DeliveryAndDebitNoteBill2> GetDeliveryAndDebitNoteBill2(string branchid, string platid, int num)
        {
            //正式库需要改此ID
            if (branchid == "FDJ")
            {
                DefinedAPI api = new DefinedAPI();
                List<ParamInfo> list = new List<ParamInfo>();
                api.SqlScript = "SELECT * FROM (SELECT * FROM bjqerp.vw_drug_base_bill_ps_fj WHERE BRANCHID =:BranchId AND PLATID =:PlatId) WHERE ROWNUM <=:Num";
                list.Add(new ParamInfo("BranchId", "String", branchid));
                list.Add(new ParamInfo("PlatId", "String", platid));
                list.Add(new ParamInfo("Num", "Int", num.ToString()));
                api.ParamList = list;
                api.IsPaged = false;
                try
                {
                    ReturnMsg<DeliveryAndDebitNoteBill2> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DeliveryAndDebitNoteBill2>>(ApiRetrunData(api));
                    if (order.ResultSet.Table.Count <= 0) return null;
                    //List<string> table = new List<string>();
                    List<DeliveryAndDebitNoteBill2> order2 = new List<DeliveryAndDebitNoteBill2>();
                    order2 = order.ResultSet.Table;
                    //for (int i = 0; i < order.ResultSet.Table.Count; i++)
                    //{
                    //    string PSDH = order.ResultSet.Table[i].PSDH;
                    //    table.Add(PSDH);
                    //}
                    //List<DeliveryAndDebitNoteBill2> order1 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DeliveryAndDebitNoteBill2>>("");
                    return order2;
                }
                catch (Exception ex)
                {
                    log.Error("获取发票信息为空的配送单：异常报错-" + ex.ToString());
                    return null;
                }
            }
            else
            {
                try
                {
                    string url = ERPAPi + "GetViewPSFJ" + "?branchid=" + branchid + "&platid=" + platid + "&num=" + num;
                    string data = Get(url);
                    data = data.Replace("\\", "");
                    data = data.Substring(1, data.Length - 2);
                    List<DeliveryAndDebitNoteBill2> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DeliveryAndDebitNoteBill2>>(data);
                    return order;
                }
                catch (Exception ex)
                {
                    log.Error("获取含有发票信息的配送单：异常报错-" + ex.ToString());
                    return null;
                }
            }
        }
        /// <summary>
        /// 获取福建退货发票上传到平台 Dev：付金林  Date：2017/06/01
        /// </summary>
        /// <param name="branchid"></param>
        /// <param name="platid"></param>
        /// <param name="num"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        public List<DeliveryAndDebitNoteFj> GetDebitNotePurchase(string branchid, string platid, int num, int zt)
        {
                DefinedAPI api = new DefinedAPI();
                List<ParamInfo> list = new List<ParamInfo>();
            //api.SqlScript = "SELECT * FROM (SELECT * FROM bjqerp.vw_drug_base_bill_ps_fj WHERE BRANCHID =:BranchId AND PLATID =:PlatId) WHERE ROWNUM <=:Num";
            //api.SqlScript = "select * from (select a.*,b.bzxx,b.thmxbh,b.thdbh, b.zxspbm, b.spbh, b.cgggxh,b.yybm, b.ggbz, b.scqymc  from  drug_base_bill_fp a,drug_base_bill_th b where platid=:BranchId and BRANCHID=:PlatId) where ROWNUM<=:Num";

           // select* from (select a.*, b.thmxbh, b.thdbh, b.cgggxh, b.ggbz, b.scqymc from drug_base_bill_fp a, drug_base_bill_th b  WHERE  a.branchid = 'FDG'/* AND a.platid ='PT0040'*/and  a.branchid = b.branchid and a.platid = b.platid ) where ROWNUM<= 200
                api.SqlScript= "select * from ( select a.*,b.thmxbh as cgjhid,b.thdbh, b.cgggxh, b.ggbz, b.scqymc，b.THDBH as cgdbh,b.ZXSPBMMC as cpmc, b.YPJX as jxmc, b.GG as ggbz,b.CGGGXH as ggbzmc, b.BZNHSL as bzzhb, b.GGBZ as bzdw,b.scqymc,b.THDJ as pfj from  drug_base_bill_fp a,drug_base_bill_th b WHERE a.branchid =:BranchId  and a.branchid = b.branchid and a.platid = b.platid) where ROWNUM<=:Num";
                list.Add(new ParamInfo("BranchId", "String", branchid));
                list.Add(new ParamInfo("PlatId", "String", platid));
                list.Add(new ParamInfo("Num", "Int", num.ToString()));
                api.ParamList = list;
                api.IsPaged = false;
                try
                {
                    ReturnMsg<DeliveryAndDebitNoteFj> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DeliveryAndDebitNoteFj>>(ApiRetrunData(api));
                    if (order.ResultSet.Table.Count <= 0) return null;
                    //List<string> table = new List<string>();
                    List<DeliveryAndDebitNoteFj> order2 = new List<DeliveryAndDebitNoteFj>();
                return order2;
                }
                catch (Exception ex)
                {
                    log.Error("获取发票信息为空的配送单：异常报错-" + ex.ToString());
                    return null;
                }
      
        }



        /// <summary>
        /// 获取含有发票信息的配送单（配送表信息+发票号+发票金额）
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="zt">要获取的状态</param>
        /// <returns></returns>
        public List<DeliveryAndDebitNoteBill> GetDeliveryAndDebitNoteBill(string branchid, string platid, int num, int zt)
        {
            try
            {
                string url = ERPAPi + "GetDrugBillPSFP" + "?branchid=" + branchid + "&platid=" + platid + "&num=" + num + "&zt=" + zt;
                string data = Get(url);
                if (branchid == "FWP")
                {
                    data = data.Replace("\\", "");
                    data = data.Substring(1, data.Length - 2);
                }
                if (data.Replace("\"", "") == "")
                    return null;
                //第二步将JSON返回值序列化成实体类
                List<DeliveryAndDebitNoteBill> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DeliveryAndDebitNoteBill>>(data);
                return order;
            }
            catch (Exception ex)
            {
                log.Error("获取含有发票信息的配送单：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 获取要向平台确认的配送单
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<DeliveryBill> GetOKDeliveryBill(string param)
        {
            string url = ERPAPi + "XXXX";
            //第一步通过ERPAPI接口获取JSON返回值
            string data = Get(url);
            //第二步将JSON返回值序列化成实体类

            List<DeliveryBill> bill = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DeliveryBill>>(data);
            return bill;
        }

        /// <summary>
        /// 往ERP回写配送信息
        /// </summary>
        /// <param name="bill">配送单实体集合</param>
        /// <returns></returns>
        public bool WriteBackDeliveryBillState(List<DeliveryBill> bill)
        {
            try
            {
                string url = ERPAPi + "UpdateDrugBaseBillPS";
                //第一步将列表生成json 
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(bill);
                //第二步调用ESB将订单信息上传到ERP
                if (Post(url, json) == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("回写配送单状态：异常报错-" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 回写配送信息状态
        /// </summary>
        /// <param name="ddmxbhs">订单明细编号，多个用逗号隔开</param>
        /// <param name="sczt">上传状态</param>
        /// <returns></returns>
        public int WriteBackDeliveryBillState(string ddmxbhs, int sczt)
        {
            try
            {
                ddmxbhs = ddmxbhs.Replace("\"", "").Replace("[", "").Replace("]", "");
                string url = ERPAPi + "UpdatePsddmxbhZT" + "?ddmxbh=" + ddmxbhs + "&zt=" + sczt;
                string data = Get(url);
                return int.Parse(data);
            }
            catch (Exception ex)
            {
                log.Error("回写配送单状态：异常报错-" + ex.ToString());
                return 0;
            }
        }


        /// <summary>
        /// 获取配送单号分组信息  20170306 yhyy加 上海器械平台用到
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="zt">要获取的状态</param>
        /// <returns></returns>
        public List<string> GetGroupPSDH(string branchid, string platid, int num, int zt)
        {
            //"SELECT * FROM (select PSDH from DRUG_BASE_BILL_PS where branchid=:BranchId and platid=:PlatId and is_sc=:ZT group by PSDH ) WHERE ROWNUM <=:Num";
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "SELECT * FROM (select PSDH from DRUG_BASE_BILL_PS where branchid=:BranchId and platid=:PlatId and is_sc=:ZT group by PSDH ) WHERE ROWNUM <=:Num";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("ZT", "Int", zt.ToString()));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DeliveryBill> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DeliveryBill>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<string> table = new List<string>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    string PSDH = order.ResultSet.Table[i].PSDH;
                    table.Add(PSDH);
                }

                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 获取平台返回的配送单编号分组信息  20170306 yhyy加 上海器械平台用到
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="issc">要获取的is_sc状态</param>
        /// <returns></returns>
        public List<string> GetGroupPSDBH(string branchid, string platid, int num, int issc)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "SELECT * FROM (SELECT * FROM (select substr(PSMXBH,0,instr(PSMXBH,'-')-1) PSMXBH from DRUG_BASE_BILL_PS where branchid=:BranchId and platid=:PlatId and is_sc=:issc) group by PSMXBH  ) where  ROWNUM <=:Num  ";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("issc", "Int", issc.ToString()));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DeliveryBill> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DeliveryBill>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<string> table = new List<string>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    string PSDBH = order.ResultSet.Table[i].PSMXBH;
                    table.Add(PSDBH);
                }

                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据配送单号获取配送单信息  20170306 yhyy加 上海器械平台用到
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="psdh">企业内部配送单号</param>
        /// <returns></returns>
        public List<DeliveryBill> GetDeliveryBillByPSDH(string branchid, string platid, string psdh)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from DRUG_BASE_BILL_PS where branchid=:BranchId and platid=:PlatId and psdh=:psdh";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("psdh", "String", psdh.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DeliveryBill> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DeliveryBill>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<DeliveryBill> table = new List<DeliveryBill>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    DeliveryBill deliveryBill = order.ResultSet.Table[i];
                    table.Add(deliveryBill);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据平台返回的配送单编号获取配送单信息  20170306 yhyy加 上海器械平台用到
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="psdbh">企业内部配送单号</param>
        /// <returns></returns>
        public List<DeliveryBill> GetDeliveryBillByPSDBH(string branchid, string platid, string psdbh)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from DRUG_BASE_BILL_PS where branchid=:BranchId and platid=:PlatId and psmxbh like :psdbh||'%'";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("psdbh", "String", psdbh.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DeliveryBill> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DeliveryBill>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<DeliveryBill> table = new List<DeliveryBill>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    DeliveryBill deliveryBill = order.ResultSet.Table[i];
                    table.Add(deliveryBill);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 从数据库获取处理的状态 Dev：付金林 2017/03/17   Dev：袁欢 2017/03/18修改   
        /// </summary>
        /// <param name="branchid">分公司标志</param>
        /// <param name="platid">平台id</param>
        /// <param name="num">获取的条目数</param>
        /// <param name="clzt">平台处理状态</param>
        /// <returns></returns>
        public List<string> GetGroupPSDBH(string branchid, string platid, int num, string clzt)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from (select * from (select substr(PSMXBH,0,instr(PSMXBH,'-')-1) PSMXBH from DRUG_BASE_BILL_PS where branchid=:BranchId and platid=:PlatId and length(psmxbh)>1  " +
                " and CLZT not in  (select * from table (split(:CLZT, ','))) ) group by PSMXBH )where ROWNUM <=:Num";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            list.Add(new ParamInfo("CLZT", "String", clzt.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DeliveryBill> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DeliveryBill>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<string> table = new List<string>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    string PSDBH = order.ResultSet.Table[i].PSMXBH;
                    table.Add(PSDBH);
                }

                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据状态字段名称获取配送单号分组信息 20170502 yhyy加 上海GPO平台用到
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="zt">要获取的状态</param>
        /// <param name="ztparamname">状态字段名</param>
        /// <returns></returns>
        public List<string> GetGroupPSDH(string branchid, string platid, int num, int zt, string ztparamname)
        {
            //"SELECT * FROM (select PSDH from DRUG_BASE_BILL_PS where branchid=:BranchId and platid=:PlatId and is_sc=:ZT group by PSDH ) WHERE ROWNUM <=:Num";
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "SELECT * FROM (select PSDH from DRUG_BASE_BILL_PS where branchid=:BranchId and platid=:PlatId and " + ztparamname + "=:ZT  AND yybm IN ( SELECT yybm FROM base_plat_yyxx WHERE isgpo=1 ) group by PSDH ) WHERE ROWNUM <=:Num";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("ZT", "Int", zt.ToString()));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DeliveryBill> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DeliveryBill>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<string> table = new List<string>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    string PSDH = order.ResultSet.Table[i].PSDH;
                    table.Add(PSDH);
                }

                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }
        #endregion


        #region 发票信息

        /// <summary>
        /// 获取发票信息
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="zt">要获取的状态</param>
        /// <param name="sfch">是否冲红 如果是-1则取所有</param>
        /// <returns></returns>
        public List<DebitNote> GetDebitNote(string branchid, string platid, int num, int zt, int sfch)
        {
            if (sfch == -1)
            {

                DefinedAPI api = new DefinedAPI();
                List<ParamInfo> list = new List<ParamInfo>();
                api.SqlScript = "select * from DRUG_BASE_BILL_FP where branchid=:BranchId and platid=:PlatId  and num=:Num and zt=:Zt and sfch=:sfch";
                list.Add(new ParamInfo("BranchId", "String", branchid));
                list.Add(new ParamInfo("PlatId", "String", platid));
                list.Add(new ParamInfo("Num", "String", num.ToString()));
                list.Add(new ParamInfo("Zt", "Int", zt.ToString()));
                list.Add(new ParamInfo("sfch", "Int", ""));
                api.ParamList = list;
                api.IsPaged = false;
                try
                {
                    ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                    if (order.ResultSet.Table.Count <= 0) return null;
                    List<DebitNote> table = new List<DebitNote>();
                    for (int i = 0; i < order.ResultSet.Table.Count; i++)
                    {

                        DebitNote debitDote = order.ResultSet.Table[i];
                        table.Add(debitDote);
                    }
                    return table;
                }
                catch (Exception ex)
                {
                    log.Error("上传订单信息：异常报错-" + ex.ToString());
                    return null;
                }
                //string a = "";
                //try
                //{
                //    20170302yhyy测试的时候注释了，API调整后需要恢复 当sfch不传的时候取所有
                //    string url = ERPAPi + "GetDrugBillFPList" + "?branchid=" + branchid + "&platid=" + platid + "&num=" + num + "&zt=" + zt + "&sfch=" + a;
                //    string url = ERPAPi + "GetDrugBillFPList" + "?branchid=" + branchid + "&platid=" + platid + "&num=" + num + "&zt=" + zt + "&sfch=0 ";
                //    string data = Get(url);
                //    第二步将JSON返回值序列化成实体类
                //    List<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DebitNote>>(data);
                //    return order;
                //}
                //catch (Exception ex)
                //{
                //    log.Error("获取发票信息：异常报错-" + ex.ToString());
                //    return null;
                //}
            }
            else
            {
                DefinedAPI api = new DefinedAPI();
                List<ParamInfo> list = new List<ParamInfo>();
                api.SqlScript = "select * from DRUG_BASE_BILL_FP where branchid=:BranchId and platid=:PlatId  and num=:Num and zt=:Zt and sfch=:Sfch";
                list.Add(new ParamInfo("BranchId", "String", branchid));
                list.Add(new ParamInfo("PlatId", "String", platid));
                list.Add(new ParamInfo("Num", "Int", num.ToString()));
                list.Add(new ParamInfo("Zt", "Int", zt.ToString()));
                list.Add(new ParamInfo("Sfch", "Int", sfch.ToString()));
                api.ParamList = list;
                api.IsPaged = false;
                try
                {
                    ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                    if (order.ResultSet.Table.Count <= 0) return null;
                    List<DebitNote> table = new List<DebitNote>();
                    for (int i = 0; i < order.ResultSet.Table.Count; i++)
                    {

                        DebitNote debitDote = order.ResultSet.Table[i];
                        table.Add(debitDote);
                    }
                    return table;
                }
                catch (Exception ex)
                {
                    log.Error("上传订单信息：异常报错-" + ex.ToString());
                    return null;
                }
                //try
                //{
                //    string url = ERPAPi + "GetDrugBillFPList" + "?branchid=" + branchid + "&platid=" + platid + "&num=" + num + "&zt=" + zt + "&sfch=" + sfch;
                //    string data = Get(url);
                //    //第二步将JSON返回值序列化成实体类
                //    List<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DebitNote>>(data);
                //    return order;
                //}
                //catch (Exception ex)
                //{
                //    log.Error("获取发票信息：异常报错-" + ex.ToString());
                //    return null;
                //}
            }
        }

        /// <summary>
        /// 根据关联明细编号获取发票信息
        /// </summary>
        /// <param name="glmxbh">关联明细编号</param>
        /// <returns></returns>
        public List<DebitNote> GetDebitNoteBymxbhs(string glmxbh)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from DRUG_BASE_BILL_FP where ddmxbh=:ddmxbh ";
            list.Add(new ParamInfo("ddmxbh", "String", glmxbh.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<DebitNote> table = new List<DebitNote>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {

                    DebitNote debitDote = order.ResultSet.Table[i];
                    table.Add(debitDote);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
            //try
            //{
            //    string url = ERPAPi + "GetDrugBillFP" + "?ddmxbh=" + glmxbh;
            //    string data = Get(url);
            //    //第二步将JSON返回值序列化成实体类
            //    List<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DebitNote>>(data);
            //    return order;
            //}
            //catch (Exception ex)
            //{
            //    log.Error("根据关联明细编号获取发票信息：异常报错-" + ex.ToString());
            //    return null;
            //}
        }

        /// <summary>
        /// 获取要向平台确认的发票
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<DebitNote> GetOKDebitNote(string param)
        {
            //此方法没有实现的 可以参考注释
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from DRUG_BASE_BILL_FP where ddmxbh=:ddmxbh ";
            list.Add(new ParamInfo("ddmxbh", "String", param.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<DebitNote> table = new List<DebitNote>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {

                    DebitNote debitDote = order.ResultSet.Table[i];
                    table.Add(debitDote);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
            ////http://10.3.2.22:3129/api/interface/GetDrugBillFP?ddmxbh=20160815000010051310 
            //string url = ERPAPi + "XXXX";
            ////第一步通过ERPAPI接口获取JSON返回值
            //string data = Get(url);
            ////第二步将JSON返回值序列化成实体类

            //List<DebitNote> bill = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DebitNote>>(data);
            //return bill;
        }

        /// <summary>
        /// 往ERP回写发票状态
        /// </summary>
        /// <param name="bill">发票实体集合</param>
        /// <returns></returns>
        public bool WriteBackDebitState(List<DebitNote> bill)
        {
            try
            {
                string url = ERPAPi + "UpdateDrugBaseBillFP";
                //第一步将列表生成json 
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(bill);
                //第二步调用ESB将订单信息上传到ERP
                if (Post(url, json) == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("回写发票状态：异常报错-" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 向 erp 上传医院收货入库单信息
        /// </summary>
        /// <param name="order">医院收货单实体集合</param>
        /// <returns></returns>
        public bool InsertGodownEntry(List<RKOrder> order)
        {
            try
            {
                string url = ERPAPi + "AddDrugBillYYRKXX";
                //第一步将列表生成json 
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(order);
                //第二步通过ERPAPI接口写入ERP
                if (Post(url, json) == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("医院收货单上传：异常报错-" + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// 获取发票号分组信息  20170306 yhyy加 上海器械平台用到
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="zt">要获取的状态</param>
        /// <param name="sfch">是否冲红 如果是-1则取所有</param>
        /// <returns></returns>
        public List<string> GetGroupFPH(string branchid, string platid, int num, int zt, int sfch)
        {
            #region 根据sfch判断 
            if (sfch == -1)
            {
                DefinedAPI api = new DefinedAPI();
                List<ParamInfo> list = new List<ParamInfo>();
                api.SqlScript = "SELECT * FROM (select FPH from DRUG_BASE_BILL_FP where branchid=:BranchId and platid=:PlatId and is_sc=:ZT)  WHERE ROWNUM <=:Num";
                list.Add(new ParamInfo("BranchId", "String", branchid));
                list.Add(new ParamInfo("PlatId", "String", platid));
                list.Add(new ParamInfo("ZT", "String", zt.ToString()));
                list.Add(new ParamInfo("Num", "Int", num.ToString()));
                api.ParamList = list;
                api.IsPaged = false;
                try
                {
                    ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                    if (order.ResultSet.Table.Count <= 0) return null;
                    List<string> table = new List<string>();
                    for (int i = 0; i < order.ResultSet.Table.Count; i++)
                    {
                        //string FPH = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(order.ResultSet.Table[i].FPH);
                        string FPH = order.ResultSet.Table[i].FPH.ToString();
                        table.Add(FPH);
                    }

                    return table;
                }
                catch (Exception ex)
                {
                    log.Error("上传订单信息：异常报错-" + ex.ToString());
                    return null;
                }
            }
            else
            {
                DefinedAPI api = new DefinedAPI();
                List<ParamInfo> list = new List<ParamInfo>();
                api.SqlScript = "SELECT * FROM (select FPH from DRUG_BASE_BILL_FP where branchid=:BranchId and platid=:PlatId and is_sc=:ZT and sfch=:sfch)  WHERE ROWNUM <=:Num";
                list.Add(new ParamInfo("BranchId", "String", branchid));
                list.Add(new ParamInfo("PlatId", "String", platid));
                list.Add(new ParamInfo("ZT", "String", zt.ToString()));
                list.Add(new ParamInfo("Num", "Int", num.ToString()));
                list.Add(new ParamInfo("sfch", "String", zt.ToString()));
                api.ParamList = list;
                api.IsPaged = false;
                try
                {
                    ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                    if (order.ResultSet.Table.Count <= 0) return null;
                    List<string> table = new List<string>();
                    for (int i = 0; i < order.ResultSet.Table.Count; i++)
                    {
                        string FPH = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(order.ResultSet.Table[i].FPH);
                        table.Add(FPH);
                    }

                    return table;
                }
                catch (Exception ex)
                {
                    log.Error("上传订单信息：异常报错-" + ex.ToString());
                    return null;
                }
            }
            #endregion
        }

        /// <summary>
        /// 获取平台返回的发票编号分组信息  20170306 yhyy加 上海器械平台用到
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="zt">要获取的状态</param>
        /// <param name="sfch">是否冲红 如果是-1则取所有</param>
        /// <returns></returns>
        public List<string> GetGroupFPBH(string branchid, string platid, int num, int zt, int sfch)
        {
            #region 根据sfch判断 
            if (sfch == -1)
            {
                DefinedAPI api = new DefinedAPI();
                List<ParamInfo> list = new List<ParamInfo>();
                api.SqlScript = "SELECT * FROM (SELECT * FROM (SELECT  substr(FPMXBH, 0, instr(FPMXBH, '-') - 1) FPMXBH from DRUG_BASE_BILL_FP where branchid = :BranchId and platid = :PlatId and is_sc = :ZT)  group by FPMXBH  )  where ROWNUM <=:Num ";
                list.Add(new ParamInfo("BranchId", "String", branchid));
                list.Add(new ParamInfo("PlatId", "String", platid));
                list.Add(new ParamInfo("ZT", "String", zt.ToString()));
                list.Add(new ParamInfo("Num", "Int", num.ToString()));
                api.ParamList = list;
                api.IsPaged = false;
                try
                {
                    ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                    if (order.ResultSet.Table.Count <= 0) return null;
                    List<string> table = new List<string>();
                    for (int i = 0; i < order.ResultSet.Table.Count; i++)
                    {
                        string FPBH = order.ResultSet.Table[i].FPMXBH;
                        table.Add(FPBH);
                    }

                    return table;
                }
                catch (Exception ex)
                {
                    log.Error("上传订单信息：异常报错-" + ex.ToString());
                    return null;
                }
            }
            else
            {
                DefinedAPI api = new DefinedAPI();
                List<ParamInfo> list = new List<ParamInfo>();
                api.SqlScript = "SELECT * FROM (SELECT * FROM (SELECT  substr(FPMXBH, 0, instr(FPMXBH, '-') - 1) FPMXBH from DRUG_BASE_BILL_FP where branchid = :BranchId and platid = :PlatId and is_sc = :ZT and sfch=:sfch ) group by FPMXBH) WHERE ROWNUM <=:Num";
                list.Add(new ParamInfo("BranchId", "String", branchid));
                list.Add(new ParamInfo("PlatId", "String", platid));
                list.Add(new ParamInfo("ZT", "String", zt.ToString()));
                list.Add(new ParamInfo("Num", "Int", num.ToString()));
                list.Add(new ParamInfo("sfch", "String", zt.ToString()));
                api.ParamList = list;
                api.IsPaged = false;
                try
                {
                    ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                    if (order.ResultSet.Table.Count <= 0) return null;
                    List<string> table = new List<string>();
                    for (int i = 0; i < order.ResultSet.Table.Count; i++)
                    {
                        string FPBH = order.ResultSet.Table[i].FPMXBH;
                        table.Add(FPBH);
                    }

                    return table;
                }
                catch (Exception ex)
                {
                    log.Error("上传订单信息：异常报错-" + ex.ToString());
                    return null;
                }
            }
            #endregion
        }

        /// <summary>
        /// 根据发票号获取发票信息  20170306 yhyy加 上海器械平台用到
        /// </summary>
        /// <param name="fph">发票号</param>
        /// <returns></returns>
        public List<DebitNote> GetDebitNoteByFPH(string branchid, string platid, string fph)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from DRUG_BASE_BILL_FP where branchid=:BranchId and platid=:PlatId  and fph=:fph";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("fph", "String", fph.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<DebitNote> table = new List<DebitNote>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {

                    DebitNote debitDote = order.ResultSet.Table[i];
                    table.Add(debitDote);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 根据平台返回的发票编号获取发票信息  20170306 yhyy加 上海器械平台用到
        /// </summary>
        /// <param name="fpbh">发票编号</param>
        /// <returns></returns>
        public List<DebitNote> GetDebitNoteByFPBH(string branchid, string platid, string fpbh)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from DRUG_BASE_BILL_FP where branchid=:BranchId and platid=:PlatId and fpmxbh like :fpbh||'%'";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("fpbh", "String", fpbh.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<DebitNote> table = new List<DebitNote>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    DebitNote debitDote = order.ResultSet.Table[i];
                    table.Add(debitDote);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 从数据库获取处理的状态 Dev：袁欢 2017/03/18
        /// </summary>
        /// <param name="branchid">分公司标志</param>
        /// <param name="platid">平台id</param>
        /// <param name="num">获取的条目数</param>
        /// <returns></returns>
        public List<string> GetGroupFPBH(string branchid, string platid, int num, string clzt)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from(select * from (select substr(FPMXBH,0,instr(FPMXBH,'-')-1) FPMXBH from DRUG_BASE_BILL_FP " +
                "where branchid=:BranchId and platid=:PlatId  and length(fpmxbh)>1 and CLZT not in  (select * from table (split(:CLZT, ',')))) group by FPMXBH )where ROWNUM <=:Num";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            list.Add(new ParamInfo("CLZT", "String", clzt.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<string> table = new List<string>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    string FPBH = order.ResultSet.Table[i].FPMXBH;
                    table.Add(FPBH);
                }

                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 向 erp 上传 从平台上获取的已关联的票据信息  Dev:袁欢 2017/3/17 福建公司用到
        /// </summary>
        /// <param name="order">实体集合</param>
        /// <returns></returns>
        public bool UploadInvoiceData(List<DebitNote> order)
        {
            try
            {
                if (order == null)
                    return false;
                string url = ERPAPi + "XXXXX";
                //第一步将列表生成json 
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(order);
                //第二步通过ERPAPI接口写入ERP
                if (Post(url, json) == "true")
                    return true;
                else
                {
                    log.Error("关联票据信息上传：请求返回失败");
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error("关联票据信息上传：异常报错-" + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// 根据状态字段名称获取发票号分组信息  20170502 yhyy加 上海GPO平台用到
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="zt">要获取的状态</param>
        /// <param name="ztparamname">状态字段名</param>
        /// <param name="sfch">是否退货冲红 1-是 0-否 -1取全部</param>
        /// <returns></returns>
        public List<string> GetGroupFPH(string branchid, string platid, int num, int zt, string ztparamname, int sfch)
        {
            #region 根据sfch判断 
            if (sfch == -1)
            {
                DefinedAPI api = new DefinedAPI();
                List<ParamInfo> list = new List<ParamInfo>();
                api.SqlScript = "SELECT * FROM (select FPH from DRUG_BASE_BILL_FP where branchid=:BranchId and platid=:PlatId and " + ztparamname + "=:ZT AND yybm IN ( SELECT yybm FROM base_plat_yyxx WHERE isgpo=1 ))  WHERE ROWNUM <=:Num";
                list.Add(new ParamInfo("BranchId", "String", branchid));
                list.Add(new ParamInfo("PlatId", "String", platid));
                list.Add(new ParamInfo("ZT", "String", zt.ToString()));
                list.Add(new ParamInfo("Num", "Int", num.ToString()));
                api.ParamList = list;
                api.IsPaged = false;
                try
                {
                    ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                    if (order.ResultSet.Table.Count <= 0) return null;
                    List<string> table = new List<string>();
                    for (int i = 0; i < order.ResultSet.Table.Count; i++)
                    {
                        //string FPH = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(order.ResultSet.Table[i].FPH);
                        string FPH = order.ResultSet.Table[i].FPH.ToString();
                        table.Add(FPH);
                    }

                    return table;
                }
                catch (Exception ex)
                {
                    log.Error("上传订单信息：异常报错-" + ex.ToString());
                    return null;
                }
            }
            else if (sfch == 1)
            {
                DefinedAPI api = new DefinedAPI();
                List<ParamInfo> list = new List<ParamInfo>();
                api.SqlScript = "SELECT * FROM (select FPH from DRUG_BASE_BILL_FP where branchid=:BranchId and platid=:PlatId and " + ztparamname + "=:ZT and sfch=:sfch AND yybm IN ( SELECT yybm FROM base_plat_yyxx WHERE isgpo=1 ))  WHERE ROWNUM <=:Num";
                list.Add(new ParamInfo("BranchId", "String", branchid));
                list.Add(new ParamInfo("PlatId", "String", platid));
                list.Add(new ParamInfo("ZT", "String", zt.ToString()));
                list.Add(new ParamInfo("Num", "Int", num.ToString()));
                list.Add(new ParamInfo("sfch", "String", sfch.ToString()));
                api.ParamList = list;
                api.IsPaged = false;
                try
                {
                    ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                    if (order.ResultSet.Table.Count <= 0) return null;
                    List<string> table = new List<string>();
                    for (int i = 0; i < order.ResultSet.Table.Count; i++)
                    {
                        //string FPH = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(order.ResultSet.Table[i].FPH);
                        string FPH = order.ResultSet.Table[i].FPH.ToString();
                        table.Add(FPH);
                    }

                    return table;
                }
                catch (Exception ex)
                {
                    log.Error("上传订单信息：异常报错-" + ex.ToString());
                    return null;
                }
            }
            else
            {
                DefinedAPI api = new DefinedAPI();
                List<ParamInfo> list = new List<ParamInfo>();
                api.SqlScript = "SELECT * FROM (select FPH from DRUG_BASE_BILL_FP where branchid=:BranchId and platid=:PlatId and " + ztparamname + "=:ZT  and sfch=:sfch AND yybm IN ( SELECT yybm FROM base_plat_yyxx WHERE isgpo=1 ))  WHERE ROWNUM <=:Num";
                list.Add(new ParamInfo("BranchId", "String", branchid));
                list.Add(new ParamInfo("PlatId", "String", platid));
                list.Add(new ParamInfo("ZT", "String", zt.ToString()));
                list.Add(new ParamInfo("Num", "Int", num.ToString()));
                list.Add(new ParamInfo("sfch", "String", sfch.ToString()));
                api.ParamList = list;
                api.IsPaged = false;
                try
                {
                    ReturnMsg<DebitNote> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNote>>(ApiRetrunData(api));
                    if (order.ResultSet.Table.Count <= 0) return null;
                    List<string> table = new List<string>();
                    for (int i = 0; i < order.ResultSet.Table.Count; i++)
                    {
                        //string FPH = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(order.ResultSet.Table[i].FPH);
                        string FPH = order.ResultSet.Table[i].FPH.ToString();
                        table.Add(FPH);
                    }

                    return table;
                }
                catch (Exception ex)
                {
                    log.Error("上传订单信息：异常报错-" + ex.ToString());
                    return null;
                }
            }
            #endregion
        }

        /// <summary>
        /// 获取调价补差发票号分组信息  20170502 yhyy加 上海GPO平台用到
        /// </summary>
        /// <param name="branchid">分公司标识</param>
        /// <param name="platid">平台ID</param>
        /// <param name="num">要获取的条目数</param>
        /// <param name="zt">要获取的状态</param>
        /// <returns></returns>
        public List<string> GetGroupFPHXSB(string branchid, string platid, int num, int zt)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "SELECT * FROM (select FPH from DRUG_BASE_BILL_XS where branchid=:BranchId and platid=:PlatId and is_sc=:ZT AND yybm IN ( SELECT yybm FROM base_plat_yyxx WHERE isgpo=1 ))  WHERE ROWNUM <=:Num";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("ZT", "String", zt.ToString()));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DebitNoteXSB> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNoteXSB>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<string> table = new List<string>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    //string FPH = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(order.ResultSet.Table[i].FPH);
                    string FPH = order.ResultSet.Table[i].FPH.ToString();
                    table.Add(FPH);
                }

                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 根据发票号获取调价补差发票信息  20170502 yhyy加 上海GPO平台用到
        /// </summary>
        /// <param name="fph">发票号</param>
        /// <returns></returns>
        public List<DebitNoteXSB> GetDebitNoteXSBByFPH(string branchid, string platid, string fph)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from DRUG_BASE_BILL_XS where branchid=:BranchId and platid=:PlatId  and fph=:fph";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("fph", "String", fph.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DebitNoteXSB> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNoteXSB>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<DebitNoteXSB> table = new List<DebitNoteXSB>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {

                    DebitNoteXSB debitDote = order.ResultSet.Table[i];
                    table.Add(debitDote);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 往ERP回写调价补差发票状态 20170502 yhyy加 上海GPO平台用到
        /// </summary>
        /// <param name="bill">发票实体集合</param>
        /// <returns></returns>
        public bool WriteBackDebitXSBState(List<DebitNoteXSB> bill)
        {
            try
            {
                string url = ERPAPi + "UpdateDrugBaseBillFP";
                //第一步将列表生成json 
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(bill);
                //第二步调用ESB将订单信息上传到ERP
                if (Post(url, json) == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("回写发票状态：异常报错-" + ex.ToString());
                return false;
            }
        }
        /// <summary>
        /// 下载图片名字
        /// </summary>
        /// <param name="branchID">分公司标志</param>
        /// <param name="TPId">平台Id</param>
        /// <param name="is_sc">是否长传</param>
        /// <param name="num">取多少条数据</param>
        /// <returns></returns>
        public List<string> DownPictureName(string branchid, string platid, int is_sc, int num)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "SELECT * FROM (select FPH from DRUG_BASE_BILL_XS where branchid=:BranchId and platid=:PlatId and is_sc=:ZT)  WHERE ROWNUM <=:Num";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("ZT", "String", is_sc.ToString()));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<DebitNoteXSB> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<DebitNoteXSB>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<string> table = new List<string>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {
                    //string FPH = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(order.ResultSet.Table[i].FPH);
                    string FPH = order.ResultSet.Table[i].FPH.ToString();
                    table.Add(FPH);
                }

                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }



        #endregion


        #region 库存信息
        /// <summary>
        /// 获取库存信息(福建专用) Dev：付金林 2017/05/08
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<Stock1> GetStockFj(string branchid, string platid, int iswc, int num)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select c.bzxx,c.spm cpmc ,c.ypjx jxmc, c.gg ggbzmc, c.bzdw,c.bzsl bzzhb, c.companyidsc scqybm, c.scqymc ,a.*  from  DRUG_BASE_STOCK a,DRUG_BASE_inf c  where c.platid=a.platid and c.branchid=a.branchid  and a.zxspbm=c.zxspbm and a.is_sc=:Is_sc and a.platid=:PlatId and a.branchid=:BranchId";//num<=:Num
            //api.SqlScript = "select c.bzxx,c.cpm,c.ypjx,c.gg,c.bzdw,c.bzsl,c.companyidsc,c.scqymc,a.* from  DRUG_BASE_STOCK a,DRUG_BASE_inf c  where c.platid=a.platid and c.branchid=a.branchid  and a.zxspbm=c.zxspbm and a.is_sc=:Is_sc";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("Is_sc", "Int", iswc.ToString()));
            //list.Add(new ParamInfo("Num", "int", num.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<Stock1> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<Stock1>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<Stock1> table = new List<Stock1>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {

                    Stock1 debitDote = order.ResultSet.Table[i];
                    table.Add(debitDote);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 获取库存信息
        /// </summary>
        /// <returns></returns>
        public List<Stock> GetStock(string param)
        {
            //http://10.3.2.22:3129/api/interface/ XXXX
            string url = ERPAPi + "XXXX";
            //第一步通过ERPAPI接口获取JSON返回值
            string data = Get(url);
            //第二步将JSON返回值序列化成实体类

            List<Stock> order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Stock>>(data);
            return order;
        }

        /// <summary>
        /// 往ERP回写库存上传状态信息
        /// </summary>
        /// <returns></returns>
        public bool WriteBackStockState(List<Stock> bill)
        {
            //http://10.3.2.22:3129/api/interface/ XXXX
            string url = ERPAPi + "XXXX";
            //第一步将列表生成json 
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(bill);
            //第二步调用ESB将订单信息上传到ERP
            if (Post(url, json) == "true")
                return true;
            else
                return false;
        }
        /// <summary>
        /// 往ERP回写库存上传状态信息
        /// </summary>
        /// <param name="bill"></param>
        /// <returns></returns>
        public bool WriteBackStockStateFj(List<DrugBaseStock> bill)
        {
            //http://10.3.2.22:3129/api/interface/ XXXX
            string url = ERPAPi + "UpdateDrugBaseStock";
            //第一步将列表生成json 
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(bill);
            //第二步调用ESB将订单信息上传到ERP
            if (Post(url, json) == "true")
                return true;
            else
                return false;
        }





        #endregion


        #region 货款支付

        /// <summary>
        /// 向 erp 上传货款支付信息  Dev:袁欢 2017/5/2 上海公司GPO对账系统用到
        /// </summary>
        /// <param name="order">订单实体集合</param>
        /// <returns></returns>
        public bool UploadPaymentOrder(List<Payment> order)
        {
            //加入分页功能
            try
            {
                if (order == null)
                    return false;
                string url = ERPAPi + "InsertSalePayment";
                string msg = "";
                for (int i = 1; i <= order.Count / 200 + 1; i++)
                {
                    List<Payment> list1 = order.Skip((i - 1) * 200).Take(200).ToList();
                    //将商品资料转换成JSON格式
                    string json1 = Newtonsoft.Json.JsonConvert.SerializeObject(list1);

                    //通过ERPAPI接口写入ERP
                    if (Post(url, json1) != "true")
                        msg += "error";
                }
                if (msg == "")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 回写货款支付单状态
        /// </summary>
        /// <param name="order">货款支付单实体集合</param>
        /// <returns></returns>
        public bool UpdatePaymentOrder(List<Payment> order)
        {
            try
            {
                string url = ERPAPi + "UpdateSalePayment";
                //第一步将列表生成json 
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(order);
                //第二步调用ESB将订单信息上传到ERP
                if (Post(url, json) == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.Error("回写订单状态：异常报错-" + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// 获取货款发票销账信息  Dev:袁欢 2017/5/2 上海公司GPO对账系统用到
        /// </summary>
        /// <returns></returns>
        public List<string> GetGroupZFDBH(string branchid, string platid, int num, int zt)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "SELECT * FROM (select zfdbh from TB_SALE_PAYMENT where branchid =:BranchId and platid =:PlatId and zfdZT =:ZT GROUP BY zfdbh)  WHERE ROWNUM <=:Num";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("ZT", "String", zt.ToString()));
            list.Add(new ParamInfo("Num", "Int", num.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<Payment> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<Payment>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<string> table = new List<string>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {

                    string zfdbh = order.ResultSet.Table[i].ZFDBH.ToString();
                    table.Add(zfdbh);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// 获取货款发票销账信息  Dev:袁欢 2017/5/2 上海公司GPO对账系统用到
        /// </summary>
        /// <returns></returns>
        public List<Payment> GetPaymentOrder(string branchid, string platid, string zfdbh)
        {
            DefinedAPI api = new DefinedAPI();
            List<ParamInfo> list = new List<ParamInfo>();
            api.SqlScript = "select * from TB_SALE_PAYMENT where branchid=:BranchId and platid=:PlatId  and zfdbh=:Zfdbh";
            list.Add(new ParamInfo("BranchId", "String", branchid));
            list.Add(new ParamInfo("PlatId", "String", platid));
            list.Add(new ParamInfo("Zfdbh", "String", zfdbh.ToString()));
            api.ParamList = list;
            api.IsPaged = false;
            try
            {
                ReturnMsg<Payment> order = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg<Payment>>(ApiRetrunData(api));
                if (order.ResultSet.Table.Count <= 0) return null;
                List<Payment> table = new List<Payment>();
                for (int i = 0; i < order.ResultSet.Table.Count; i++)
                {

                    Payment debitDote = order.ResultSet.Table[i];
                    table.Add(debitDote);
                }
                return table;
            }
            catch (Exception ex)
            {
                log.Error("上传订单信息：异常报错-" + ex.ToString());
                return null;
            }
        }



        #endregion
    }
}

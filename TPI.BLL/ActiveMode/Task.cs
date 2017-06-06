using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Net;
using System.IO;
using Quartz;
using Quartz.Impl;
using TPI.Common;
using TPI.BLL.ActiveMode;
using log4net;

namespace TPI.BLL
{
    /// <summary>
    /// 公司-交易平台，主动模式下的配置文件，
    /// 定义了每个任务的相关参数及定时时间等信息。
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(HZTEST))]
    [XmlInclude(typeof(HZGOODS))]
    [XmlInclude(typeof(HZGOODSEXIST))]
    [XmlInclude(typeof(HZHOSPITALS))]
    [XmlInclude(typeof(HZHOSPITALSEXIST))]
    [XmlInclude(typeof(HZCOMPANYS))]
    [XmlInclude(typeof(HZORDER))]
    [XmlInclude(typeof(HZORDERREAD))]
    [XmlInclude(typeof(HZORDERRESPONSE))]
    [XmlInclude(typeof(HZORDERNOTRESPONSE))]
    [XmlInclude(typeof(HZPURCHASE))]
    [XmlInclude(typeof(HZRKORDER))]
    [XmlInclude(typeof(HZRETURNORDER))]
    [XmlInclude(typeof(HZRETURNORDERRESPONSE))]
    [XmlInclude(typeof(HZRETURNORDERNOTRESPONSE))]
    [XmlInclude(typeof(HZRETURNDEBITNOTE))]

    [XmlInclude(typeof(HNTEST))]
    [XmlInclude(typeof(HNGOODS))]
    [XmlInclude(typeof(HNHOSPITALS))]
    [XmlInclude(typeof(HNORDER))]
    [XmlInclude(typeof(HNPURCHASE))]
    [XmlInclude(typeof(HNRKORDER))]

    [XmlInclude(typeof(CCTEST))]
    [XmlInclude(typeof(CCGOODS))]
    [XmlInclude(typeof(CCHOSPITALS))]
    [XmlInclude(typeof(CCORDER))]
    [XmlInclude(typeof(CCRETURNORDER))]
    [XmlInclude(typeof(CCRETURNORDERRESPONSE))]
    [XmlInclude(typeof(CCPURCHASE))]
    [XmlInclude(typeof(CCSTOCK))]


    [XmlInclude(typeof(SHQXHOSPITALS))]
    [XmlInclude(typeof(SHQXUPDATEHOSPITAL))]
    [XmlInclude(typeof(SHQXORDER))]
    [XmlInclude(typeof(SHQXORDERRESPONSE))]
    [XmlInclude(typeof(SHQXORDEROVERQUEREN))]
    [XmlInclude(typeof(SHQXORDERWRITE))]
    [XmlInclude(typeof(SHQXORDERWRITE1))]
    [XmlInclude(typeof(SHQXORDERWRITEQUEREN))]
    [XmlInclude(typeof(SHQXRETURNORDER))]
    [XmlInclude(typeof(SHQXRETURNORDERRESPONSE))]
    [XmlInclude(typeof(SHQXRETURNORDERWRITE))]
    [XmlInclude(typeof(SHQXRETURNORDERWRITE1))]
    [XmlInclude(typeof(SHQXRETURNORDERWRITEQUEREN))]
    [XmlInclude(typeof(SHQXPURCHASE))]
    [XmlInclude(typeof(SHQXPURCHASE1))]
    [XmlInclude(typeof(SHQXPURCHASEQUEREN))]
    [XmlInclude(typeof(SHQXPURCHASESTATUS))]
    [XmlInclude(typeof(SHQXDEBITNOTE))]
    [XmlInclude(typeof(SHQXDEBITNOTE1))]
    [XmlInclude(typeof(SHQXDEBITNOTEQUEREN))]
    [XmlInclude(typeof(SHQXDEBITNOTESTATUS))]
    [XmlInclude(typeof(SHQXSTOCK))]



    [XmlInclude(typeof(FJTEST))]
    [XmlInclude(typeof(FJGOODS))]
    [XmlInclude(typeof(FJSTOCK))]
    [XmlInclude(typeof(FJORDER))]
    [XmlInclude(typeof(FJPURCHASE))]
    [XmlInclude(typeof(FJINVOICE))]
    [XmlInclude(typeof(FJRETURNORDER))]
    [XmlInclude(typeof(FJINVOICEDATAGET))]
    [XmlInclude(typeof(FJINVOICEDATAUPLOAD))]


    [XmlInclude(typeof(BJTEST))]
    [XmlInclude(typeof(BJORDER))]
    [XmlInclude(typeof(BJPURCHASE))]


    [XmlInclude(typeof(SHGPOTEST))]
    [XmlInclude(typeof(SHGPOPURCHASE))]
    [XmlInclude(typeof(SHGPODEBITNOTE))]
    [XmlInclude(typeof(SHGPODEBITNOTE1))]
    [XmlInclude(typeof(SHGPODEBITNOTEXSB))]
    [XmlInclude(typeof(SHGPOPAYMENTDOWNLOAD))]
    [XmlInclude(typeof(SHGPOPAYMENTUPLOAD))]
    public class TaskBranch
    {
        /// <summary>
        /// 每个公司及对应交易平台的名字， 命名规则为
        /// 公司-平台名 如 FWP-ChangCun
        /// 每个组名会作为调度任务中的一个组。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 分公司 ID
        /// </summary>
        public string BranchID { get; set; }

        /// <summary>
        /// 外部交易 平台 ID 
        /// </summary>
        public string TPID { get; set; }

        public bool IsEnable { get; set; }


        public List<BaseTask> TaskList { get; set; }

        public TaskBranch()
        {
            TaskList = new List<BaseTask>();
        }

        /// <summary>
        /// 初始化任务及创建任务
        /// 创建日志
        /// </summary>
        public void Initial()
        {
            if (!IsEnable)
                return;
            foreach (BaseTask item in TaskList)
            {
                //创建业务对象
                AcitveModeService servie = new AcitveModeService(BranchID, TPID);
                item.Initial(servie);

                //创建日志。
                item.CreateLosg(Name);
            }
        }

        public void Start()
        {
            foreach (BaseTask item in TaskList)
            {
            }
        }

        public void Stop()
        { }

        public void CreaTasks()
        {
            if (!IsEnable)
                return;
            foreach (BaseTask item in TaskList)
            {
                CreateTask(item);
            }

        }

        private void CreateTask(BaseTask task)
        {
            IJobDetail job = JobBuilder.Create(typeof(Task)).WithIdentity(string.Format("job_{0}", task.Name), Name).Build();

            job.JobDataMap.Add("Updater", task);

            ITrigger trigger = TriggerBuilder.Create()
    .WithIdentity(string.Format("trigger_{0}", task.Name), Name)
    .StartAt(DateBuilder.EvenMinuteDate(DateTimeOffset.UtcNow))
    .WithSimpleSchedule(x => x.WithIntervalInSeconds(task.ScheduleTime).RepeatForever())
    .Build();

            TaskManager.GetInstance().Scheduler.ScheduleJob(job, trigger);
        }
    }
    public enum ExeState
    {
        Idle,
        Busy
    }
    [Serializable]
    public class BaseTask
    {
        protected AcitveModeService acitveModeService;

        #region 记录日志

        /// <summary>
        /// log 消息由外界传进来
        /// </summary>
        [XmlIgnore]
        protected ILog log;

        public void CreateLosg(string logname)
        {
            log = log4net.LogManager.GetLogger(logname);
            acitveModeService.SetLog(log);
            acitveModeService.SetERPUrl();
            acitveModeService.SetCustomERPUrl();
            acitveModeService.SetTPUrl();

        }

        #endregion

        [XmlAttribute]
        public string Name { get; set; }
        /// <summary>
        /// 本任务的简要说明
        /// </summary>
        [XmlAttribute]
        public string Text { get; set; }

        [XmlAttribute]
        public bool IsEnable { get; set; }

        [XmlAttribute]
        public int ScheduleTime { get; set; }

        [XmlIgnore]
        public ExeState State { get; set; }

        public BaseTask()
        {
            State = ExeState.Idle;
        }
        public void Initial(AcitveModeService service)
        {
            acitveModeService = service;
        }
        protected virtual void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1}", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1}", Name, ScheduleTime));
        }

        public virtual void Exe()
        {
            if (!IsEnable)
                return;

            if (State == ExeState.Busy)
            {
                return;
            }

            State = ExeState.Busy;

            try
            {
                Execute();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("{0}-{1}-{2}", Name, ScheduleTime, ex.ToString()));
                throw ex;
            }
            finally
            {
                State = ExeState.Idle;
            }
        }
    }

    #region 杭州公司任务

    #region 测试框架用

    //杭州test
    public class HZTEST : BaseTask
    {
        protected override void Execute()
        {
            log.Info(string.Format("{0}-{1} override task 1 ------------------------------", Name, ScheduleTime));


            acitveModeService.HZTEST();
        }
    }
    #endregion

    #region 基础资料

    //杭州商品资料
    public class HZGOODS : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 1", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 1 ------------------------------", Name, ScheduleTime));
            

            acitveModeService.HZGOOD();
        }
    }
    //杭州医院信息
    public class HZHOSPITALS : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 2", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 2 ----------------------------", Name, ScheduleTime));


            acitveModeService.HZHOSPITAL();
        }
    }
    //杭州厂家信息
    public class HZCOMPANYS : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 3", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 3 ----------------------------", Name, ScheduleTime));


            acitveModeService.HZCOMPANY();
        }
    }


    //杭州商品资料是否存在
    public class HZGOODSEXIST : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 1", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 1 ------------------------------", Name, ScheduleTime));


            acitveModeService.HZGOODEXIST();
        }
    }

    //杭州医院信息是否存在
    public class HZHOSPITALSEXIST : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 2", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 2 ----------------------------", Name, ScheduleTime));


            acitveModeService.HZHOSPITALEXIST();
        }
    }
    #endregion

    #region 订单
    //杭州订单下载
    public class HZORDER : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 4", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 4 ------------------------------", Name, ScheduleTime));



            acitveModeService.HZORDER();
        }


    }
    //杭州订单阅读
    public class HZORDERREAD : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 5", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 5 ----------------------------", Name, ScheduleTime));


            acitveModeService.HZORDERREAD();
        }
    }
    //杭州订单响应
    public class HZORDERRESPONSE : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 6", Name, ScheduleTime));

            log.Info(string.Format("{0}-{1} override task 6 ----------------------------", Name, ScheduleTime));


            acitveModeService.HZORDERRESPONSE();
        }
    }
    //杭州订单拒绝响应
    public class HZORDERNOTRESPONSE : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 6", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 6 ----------------------------", Name, ScheduleTime));


            acitveModeService.HZORDERNOTRESPONSE();
        }
    }
    #endregion

    #region 配送单
    //杭州配送单上传
    public class HZPURCHASE : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 7", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1} override task 7 ------------------------------", Name, ScheduleTime));
            acitveModeService.HZPURCHASE();
        }
    }
    #endregion

    #region 收货单

    //杭州收货单下载
    public class HZRKORDER : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 8", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 8 ----------------------------", Name, ScheduleTime));


            acitveModeService.HZRKORDER();
        }
    }
    #endregion

    #region 退货单
    //杭州退货单下载
    public class HZRETURNORDER : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 9", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 9 ----------------------------", Name, ScheduleTime));


            acitveModeService.HZRETURNORDER();
        }
    }
    //杭州退货单响应
    public class HZRETURNORDERRESPONSE : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 10", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 10 ------------------------------", Name, ScheduleTime));



            acitveModeService.HZRETURNORDERRESPONSE();
        }


    }
    //杭州退货单拒绝响应
    public class HZRETURNORDERNOTRESPONSE : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 10", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 10 ------------------------------", Name, ScheduleTime));



            acitveModeService.HZRETURNORDERNOTRESPONSE();
        }


    }
    #endregion

    #region 发票
    //杭州退货发票上传
    public class HZRETURNDEBITNOTE : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 11", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 11 ----------------------------", Name, ScheduleTime));


            acitveModeService.HZRETURNDEBITNOTE();
        }
    }
    #endregion
    #endregion


    #region 河南公司任务

    #region 测试框架用

    //河南test
    public class HNTEST : BaseTask
    {
        protected override void Execute()
        {
            log.Info(string.Format("{0}-{1} override task 1 ------------------------------", Name, ScheduleTime));


            acitveModeService.HNTEST();
        }
    }
    #endregion


    #region 基础资料

    //河南商品资料
    public class HNGOODS : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 1", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1} override task 1 ------------------------------", Name, ScheduleTime));
            acitveModeService.HNGOOD();
        }
    }
    //河南医院信息
    public class HNHOSPITALS : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 2", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1} override task 2 ----------------------------", Name, ScheduleTime));
            acitveModeService.HNHOSPITAL();
        }
    }
    #endregion

    #region 订单
    //河南订单下载
    public class HNORDER : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 4", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 4 ------------------------------", Name, ScheduleTime));



            acitveModeService.HNORDER();
        }


    }
    #endregion

    #region 配送单
    //河南配送单上传
    public class HNPURCHASE : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 7", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 7 ------------------------------", Name, ScheduleTime));



            acitveModeService.HNPURCHASE();
        }


    }
    #endregion

    #region 收货单

    //河南收货单下载
    public class HNRKORDER : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 8", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 8 ----------------------------", Name, ScheduleTime));


            acitveModeService.HNRKORDER();
        }
    }
    #endregion

    #endregion


    #region 长春公司任务

    #region 测试框架用

    //长春test
    public class CCTEST : BaseTask
    {
        protected override void Execute()
        {
            log.Info(string.Format("{0}-{1} override task 1 ------------------------------", Name, ScheduleTime));


            acitveModeService.CCTEST();
        }
    }
    #endregion

    #region 基础资料

    //长春商品资料
    public class CCGOODS : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 1", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1} override task 1 ------------------------------", Name, ScheduleTime));
            acitveModeService.CCGOOD();
        }
    }
    //长春医院信息
    public class CCHOSPITALS : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 2", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1} override task 2 ----------------------------", Name, ScheduleTime));
            acitveModeService.CCHOSPITAL();
        }
    }
    #endregion

    #region 订单
    //长春订单下载
    public class CCORDER : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 4", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 4 ------------------------------", Name, ScheduleTime));



            acitveModeService.CCORDER();
        }


    }
    #endregion


    #region 退货单
    //长春退货单下载
    public class CCRETURNORDER : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 9", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 9 ----------------------------", Name, ScheduleTime));


            acitveModeService.CCRETURNORDER();
        }
    }
    //长春退货单响应
    public class CCRETURNORDERRESPONSE : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 10", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 10 ------------------------------", Name, ScheduleTime));



            acitveModeService.CCRETURNORDERRESPONSE();
        }


    }
    //长春退货单拒绝响应
    public class CCRETURNORDERNOTRESPONSE : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 10", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 10 ------------------------------", Name, ScheduleTime));



            acitveModeService.CCRETURNORDERNOTRESPONSE();
        }


    }
    #endregion

    #region 配送单
    //长春配送单上传
    public class CCPURCHASE : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 7", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 7 ------------------------------", Name, ScheduleTime));



            acitveModeService.CCPURCHASE();
        }


    }
    #endregion

    #region 库存

    //长春库存上传
    public class CCSTOCK : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 8", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 8 ----------------------------", Name, ScheduleTime));


            acitveModeService.CCSTOCK();
        }
    }
    #endregion

    #endregion


    #region 上海器械公司任务

    #region 基础资料

    //上海器械医院配送点信息
    public class SHQXHOSPITALS : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 2", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1} override task 2 ----------------------------", Name, ScheduleTime));
            acitveModeService.SHQXHOSPITAL();
        }
    }

    //将医院信息的是否获取配送点状态重置
    public class SHQXUPDATEHOSPITAL : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 2", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1} override task 2 ----------------------------", Name, ScheduleTime));
            acitveModeService.SHQXUPDATEHOSPITAL();
        }
    }
    #endregion

    #region 订单
    //订单下载
    public class SHQXORDER : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXORDER();
        }
    }
    //订单响应
    public class SHQXORDERRESPONSE : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXORDERRESPONSE();
        }
    }
    //订单完成后确认
    public class SHQXORDEROVERQUEREN : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXORDEROVERQUEREN();
        }
    }
    //订单自填上传
    public class SHQXORDERWRITE : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXORDERWRITE();
        }
    }
    //订单自填作废
    public class SHQXORDERWRITE1 : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXORDERWRITE1();
        }
    }
    //订单自填确认
    public class SHQXORDERWRITEQUEREN : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXORDERWRITEQUEREN();
        }
    }
    #endregion


    #region 退货单
    //退单下载
    public class SHQXRETURNORDER : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXRETURNORDER();
        }
    }
    //退单响应
    public class SHQXRETURNORDERRESPONSE : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXRETURNORDERRESPONSE();
        }


    }
    //退单自填上传
    public class SHQXRETURNORDERWRITE : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXRETURNORDERWRITE();
        }
    }
    //退单自填作废
    public class SHQXRETURNORDERWRITE1 : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXRETURNORDERWRITE1();
        }
    }
    //退单自填确认
    public class SHQXRETURNORDERWRITEQUEREN : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXRETURNORDERWRITEQUEREN();
        }
    }
    #endregion

    #region 配送单
    //配送单上传
    public class SHQXPURCHASE : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXPURCHASE();
        }
    }
    //配送单作废
    public class SHQXPURCHASE1 : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXPURCHASE1();
        }
    }
    //配送单上传确认
    public class SHQXPURCHASEQUEREN : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXPURCHASEQUEREN();
        }
    }
    //配送单明细状态获取
    public class SHQXPURCHASESTATUS : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXPURCHASESTATUS();
        }
    }
    #endregion

    #region 发票
    //发票上传
    public class SHQXDEBITNOTE : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXDEBITNOTE();
        }
    }
    //发票作废
    public class SHQXDEBITNOTE1 : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXDEBITNOTE1();
        }
    }
    //发票确认
    public class SHQXDEBITNOTEQUEREN : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXDEBITNOTEQUEREN();
        }
    }
    //发票明细状态获取
    public class SHQXDEBITNOTESTATUS : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXDEBITNOTESTATUS();
        }
    }
    #endregion

    #region 库存

    //库存上传
    public class SHQXSTOCK : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHQXSTOCK();
        }
    }
    #endregion

    #endregion


    #region 福建公司任务

    #region 测试框架用

    //test
    public class FJTEST : BaseTask
    {
        protected override void Execute()
        {
            log.Info(string.Format("{0}-{1} override task 1 ------------------------------", Name, ScheduleTime));


            acitveModeService.FJTEST();
        }
    }
    #endregion


    #region 基础资料

    //商品资料
    public class FJGOODS : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 1", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1} override task 1 ------------------------------", Name, ScheduleTime));
            acitveModeService.FJGOOD();
        }
    }
    //库存上报
    public class FJSTOCK : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 2", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1} override task 2 ----------------------------", Name, ScheduleTime));
            acitveModeService.FJSTOCK();
        }
    }
    //本企业退货信息（退货发票上传） Dev:付金林 2017/4/12
    public class FJINVOICE : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.FJInvoice();
        }
    }
    #endregion


    #region 订单
    //订单下载
    public class FJORDER : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 4", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 4 ------------------------------", Name, ScheduleTime));



            acitveModeService.FJORDER();
        }


    }
    #endregion


    #region 配送单
    //福建配送单上传
    public class FJPURCHASE : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 7", Name, ScheduleTime));

            //log.Info(string.Format("{0}-{1} override task 7 ------------------------------", Name, ScheduleTime));
            
            acitveModeService.FJPURCHASE();
        }


    }
    #endregion

    #region 退货单

    //福建退货单下载
    public class FJRETURNORDER : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 8", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1} override task 8 ----------------------------", Name, ScheduleTime));
            acitveModeService.FJRETURNORDER();
        }
    }
    #endregion

    #region 两票制的票据信息

    //获取平台上已关联的票据信息
    public class FJINVOICEDATAGET : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 8", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1} override task 8 ----------------------------", Name, ScheduleTime));
            acitveModeService.FJINVOICEDATAGET();
        }
    }

    //票据信息上传
    public class FJINVOICEDATAUPLOAD : BaseTask
    {
        protected override void Execute()
        {
            //Console.WriteLine(string.Format("{0}-{1} override task 8", Name, ScheduleTime));
            //log.Info(string.Format("{0}-{1} override task 8 ----------------------------", Name, ScheduleTime));
            acitveModeService.FJINVOICEDATAUPLOAD();
        }
    }
    #endregion

    #endregion


    #region 北京公司任务

    #region 测试框架用

    //test
    public class BJTEST : BaseTask
    {
        protected override void Execute()
        {
            log.Info(string.Format("{0}-{1} override task 1 ------------------------------", Name, ScheduleTime));


            acitveModeService.BJTEST();
        }
    }
    #endregion

    #region 订单下载
    public class BJORDER : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.BJORDER();
        }
    }
    #endregion

    #region 订单配送
    public class BJPURCHASE : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.BJPURCHASE();
        }
    }
    #endregion

    #endregion


    #region 上海GPO任务

    #region 测试框架用

    //test
    public class SHGPOTEST : BaseTask
    {
        protected override void Execute()
        {
            log.Info(string.Format("{0}-{1} override task 1 ------------------------------", Name, ScheduleTime));


            acitveModeService.SHGPOTEST();
        }
    }
    #endregion

    #region 配送单
    //配送单上传
    public class SHGPOPURCHASE : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHGPOPURCHASE();
        }
    }
    #endregion

    #region 发票
    //正常发票上传
    public class SHGPODEBITNOTE : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHGPODEBITNOTE();
        }
    }

    //退货冲红发票上传
    public class SHGPODEBITNOTE1 : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHGPODEBITNOTE1();
        }
    }

    //调价补差发票上传
    public class SHGPODEBITNOTEXSB : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHGPODEBITNOTEXSB();
        }
    }
    #endregion

    #region 货款支付
    //货款支付信息下载
    public class SHGPOPAYMENTDOWNLOAD : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHGPOPAYMENTDOWNLOAD();
        }
    }

    //货款发票销账信息上传
    public class SHGPOPAYMENTUPLOAD : BaseTask
    {
        protected override void Execute()
        {
            acitveModeService.SHGPOPAYMENTUPLOAD();
        }
    }
    #endregion

    #endregion



    public class TaskBranchTest
    {
        //public TaskBranch Create()
        //{
        //    TaskBranch taskBranch = new TaskBranch() { Name = "FWP-ChangCun", IsEnable = true };

        //    BaseTask task1 = new BaseTask() { Name = "name1", Text = "任务1", IsEnable = true, ScheduleTime = 2 };
        //    BaseTask task2 = new BaseTask() { Name = "name2", Text = "任务2", IsEnable = true, ScheduleTime = 3 };
        //    Task1 task3 = new Task1() { Name = "name3", Text = "任务1", IsEnable = true, ScheduleTime = 4 };
        //    Task2 task4 = new Task2() { Name = "name4", Text = "任务2", IsEnable = true, ScheduleTime = 5 };


        //    taskBranch.TaskList.Add(task1);
        //    taskBranch.TaskList.Add(task2);
        //    taskBranch.TaskList.Add(task3);
        //    taskBranch.TaskList.Add(task4);

        //    return taskBranch;
        //}

        //public void CreateFile()
        //{
        //    TaskBranch taskBranch = Create();
        //    SerializeXML(taskBranch, taskBranch.Name + ".XML");

        //}

        #region tools

        //private void SerializeXML(object objectToConvert, string path)
        //{
        //    if (objectToConvert != null)
        //    {
        //        Type t = objectToConvert.GetType();

        //        XmlSerializer ser = new XmlSerializer(t);

        //        using (StreamWriter writer = new StreamWriter(path))
        //        {
        //            ser.Serialize(writer, objectToConvert);
        //            writer.Close();
        //        }
        //    }
        //}

        //private object DeserializeXml(string fileName, Type tp)
        //{
        //    if (!File.Exists(fileName)) return null;
        //    XmlSerializer ser = new XmlSerializer(tp);
        //    object o = null;
        //    try
        //    {
        //        using (StreamReader read = new StreamReader(fileName))
        //        {
        //            o = ser.Deserialize(read);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return o;
        //}

        #endregion

    }



}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;

namespace TPI.BLL
{
    /// <summary>
    /// 统一管理所有公司的计划任务。
    /// </summary>
    public class TaskManager
    {
        #region 单例 

        private static TaskManager _instance;

        private TaskManager()
        {
            //创建 调度管理器。
            //penghuan 2017.05.04 start 
            //定义Quartz远程实例名称和TCP端口
            var properties = new NameValueCollection();
            properties["quartz.scheduler.instanceName"] = "TradePlatformInterfaceScheduler";

            // set thread pool info
            properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
            properties["quartz.threadPool.threadCount"] = "5";
            properties["quartz.threadPool.threadPriority"] = "Normal";

            // set remoting expoter
            properties["quartz.scheduler.exporter.type"] = "Quartz.Simpl.RemotingSchedulerExporter, Quartz";
            properties["quartz.scheduler.exporter.port"] = "555";        
            properties["quartz.scheduler.exporter.bindName"] = "TradePlatformInterfaceScheduler";
            properties["quartz.scheduler.exporter.channelType"] = "tcp";
            ISchedulerFactory sf = new StdSchedulerFactory(properties);
            //penghuan 2017.05.04 end 

            Scheduler = sf.GetScheduler();

            taskBranchList = new TaskBranchCreater().Load(); //加载配置文件

        }

        public static TaskManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new TaskManager();
            }

            return _instance;
        }

        #endregion

        private List<TaskBranch> taskBranchList;

        public IScheduler Scheduler { get; set; }
        public void Start()
        {
            //TaskBranch taskBranch = new TaskBranchTest().Create();

            foreach (TaskBranch item in taskBranchList)
            {
                item.CreaTasks();
            }

            Scheduler.Start();
        }


        public void Stop()
        {
            Scheduler.Shutdown(true);
        }

    }

    /// <summary>
    /// xml 存取对象
    /// </summary>
    public class TaskBranchCreater
    {
        private string path = @"Config\ActiveMode";

        public TaskBranchCreater()
        {
            //检查目录是否存在
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private string[] GetBranchIDfils()
        {
            string[] ss = Directory.GetFiles(path);

            List<string> branchIDs = new List<string>();

            foreach (string filename in ss)
            {
                branchIDs.Add(filename);
            }

            return branchIDs.ToArray();
        }

        public List<TaskBranch> Load()
        {
            List<TaskBranch> list = new List<TaskBranch>();

            string[] files = GetBranchIDfils();

            foreach (string filename  in files)
            {
                TaskBranch taskBranch = DeserializeXml(filename, typeof(TaskBranch)) as TaskBranch;

                taskBranch.Initial();


                list.Add(taskBranch);
            }

            return list;
        }

        #region tools

        private void SerializeXML(object objectToConvert, string path)
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

        private object DeserializeXml(string fileName, Type tp)
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

        #endregion




    }

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net.Config;
using TPI.BLL;
using TPI.Common;
using TPI.Common.Busi;
using System.Text;
using System.Net;
using Newtonsoft.Json;

namespace TPI.Server
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //string a = System.DateTime.Now.ToString().Substring(0, 10);
            XmlConfigurator.Configure(new FileInfo(@"Config\Log\log4net.config"));

            MainApplication ma = MainApplication.GetInstance();
            ma.Start();


            TaskManager taskManager = TaskManager.GetInstance();
            taskManager.Start();

        



            Application.Run(new Form1());




            //TaskManager.GetInstance().Stop();
        }


        private static string Get(string url)
        {
            try
            {
                System.Net.WebRequest myHttpWebRequest = System.Net.WebRequest.Create(url);
                myHttpWebRequest.Method = "get";


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
                        }
                    }
                }
                return readerMessage;
            }
        }

    }
}

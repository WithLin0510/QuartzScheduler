using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using TPI.BLL;
using TPI.Common;
using TPI.Common.Busi;
using TPI.Common.Environment;

namespace TPI.Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public EnvironmentConfig EnvironmentConfig { get; set; }

        /// <summary>
        /// 根据商品ID从平台获取商品资料
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

            //河南 根据药品ID获取药品信息
            MainApplication ma = MainApplication.GetInstance();
            ma.Start();
            ITPProxy tpProxy = ma.EnvironmentConfig.CreateTPProxy("FDW", "PT0029");
            int iswc = 0;
            List<Goods> order = tpProxy.GetProductInfoByID("FDW", "PT0029", textBox1.Text);
            ErpProxy erp = new ErpProxy();
            string POSTURL = "http://10.36.5.66:20361/api/interface/";
            erp.InsertProductInfobyid(order, POSTURL);
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
        private void button2_Click(object sender, EventArgs e)
        {


            //EnvironmentConfig = DeserializeXml(@"Config\EnvironmentConfig.xml", typeof(EnvironmentConfig)) as EnvironmentConfig;
            //EnvironmentConfig.InitalData();


            //TaskManager taskManager = TaskManager.GetInstance();
            //taskManager.Start();

            //taskBranchList = new TaskBranchCreater().Load(); //加载配置文件
        }
    }
}

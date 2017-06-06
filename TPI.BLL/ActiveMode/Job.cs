using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace TPI.BLL
{
    public class Task : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            BaseTask updater = dataMap["Updater"] as BaseTask;

            updater.Exe();

        }
    }
}

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodInspector
{
    public class Functions
    {
        public static void ProcessMessageOnTimer(
            [TimerTrigger("0 */15 * * * *", RunOnStartup = true)] TimerInfo timerInfo,
            ILogger logger)
        {
            logger.LogInformation("TimerTrigger fired.");
        }
    }
}

using FoodInspector.InspectionDataWriter;
using FoodInspector.KeyVaultProvider;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FoodInspector
{
    public class Functions
    {

        private readonly IInspectionDataWriter _inspectionDataWriter;

        public Functions(IInspectionDataWriter inspectionDataWriter)
        {
            _inspectionDataWriter = inspectionDataWriter;
        }

        public async Task ProcessMessageOnTimer(
            [TimerTrigger("0 */15 * * * *", RunOnStartup = true)] TimerInfo timerInfo,
            ILogger logger)
        {
            logger.LogInformation("[ProcessMessageOnTimer] TimerTrigger fired.");

            try
            {
                await _inspectionDataWriter.UpsertData();
            }
            catch (Exception ex)
            {
                logger.LogInformation($"[ProcessMessageOnTimer] An exception was caught. Exception: {ex}");
            }

            logger.LogInformation("[ProcessMessageOnTimer] Processing completed.");
        }
    }
}

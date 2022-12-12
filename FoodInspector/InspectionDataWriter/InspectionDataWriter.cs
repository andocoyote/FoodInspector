using FoodInspector.KeyVaultProvider;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodInspector.InspectionDataWriter
{
    public class InspectionDataWriter : IInspectionDataWriter
    {
        private readonly IKeyVaultProvider _keyVaultProvider;
        public ILogger _logger { get; set; }
        public InspectionDataWriter(IKeyVaultProvider keyVaultProvider, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<InspectionDataWriter>();

            _keyVaultProvider = keyVaultProvider;

            try
            {
                Initialize().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"[InspectionDataWriter] An exception was caught. Exception: {ex}");
            }
        }

        public async Task UpsertData()
        {
            _logger.LogInformation("Upsert called.");
        }

        private async Task Initialize()
        {
            string apptoken = await _keyVaultProvider.GetAppToken();

            _logger.LogInformation($"[Initialize] AppToken: {apptoken}");
        }
    }
}

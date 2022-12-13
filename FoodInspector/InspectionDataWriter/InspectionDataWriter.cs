using FoodInspector.KeyVaultProvider;
using HttpClientTest.HttpHelpers;
using HttpClientTest.Model;
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
        private readonly ICommonServiceLayerProvider _commonServiceLayerProvider;
        private readonly IKeyVaultProvider _keyVaultProvider;
        private readonly ILogger _logger;

        public InspectionDataWriter(
            ICommonServiceLayerProvider commonServiceLayerProvider,
            IKeyVaultProvider keyVaultProvider,
            ILoggerFactory loggerFactory)
        {
            _commonServiceLayerProvider = commonServiceLayerProvider;
            _keyVaultProvider = keyVaultProvider;
            _logger = loggerFactory.CreateLogger<InspectionDataWriter>();

            
        }

        public async Task UpsertData()
        {
            _logger.LogInformation("Upsert called.");

            List<InspectionData> inspectionData = await _commonServiceLayerProvider.GetInspections("", "Redmond", "2022-01-01");

            _logger.LogInformation($"{inspectionData.Count} records found.");
        }
    }
}

﻿using FoodInspector.Providers.EstablishmentsProvider;
using FoodInspector.Providers.EstablishmentsTableProvider;
using FoodInspectorModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FoodInspector.InspectionDataGatherer
{
    public class InspectionDataGatherer : IInspectionDataGatherer
    {
        private readonly IEstablishmentsTableProvider _storageTableProvider;
        private readonly ILogger _logger;
        private static HttpClient _httpClient;

        public InspectionDataGatherer(
            IEstablishmentsTableProvider storageTableProvider,
            IHttpClientFactory httpClientfactory,
            ILoggerFactory loggerFactory)
        {
            _storageTableProvider = storageTableProvider;
            _httpClient = httpClientfactory.CreateClient("InspectionDataGatherer");
            _logger = loggerFactory.CreateLogger<InspectionDataGatherer>();
        }

        public async Task<List<InspectionRecordAggregated>> QueryAllInspections()
        {
            List<InspectionRecordAggregated> inspectionRecordAggregated = null;

            // Populate the table of establishments from the text file
            await _storageTableProvider.CreateEstablishmentsSet();

            // Read the set of establishment properties from the table to query
            List<EstablishmentsModel> establishmentsList = await _storageTableProvider.GetEstablishmentsSet();

            foreach (EstablishmentsModel establishment in establishmentsList)
            {
                _logger.LogInformation(
                    "[GatherData]: " +
                    $"PartitionKey: {establishment.PartitionKey} " +
                    $"RowKey: {establishment.RowKey} " +
                    $"Name: {establishment.Name} " +
                    $"City: {establishment.City}");
            }

            // Query the API to obtain the food inspection records
            using HttpResponseMessage response = await _httpClient.GetAsync("api/FoodInspector/DefaultQueries/AllEstablishmentsLatestInspectionsAggregated");

            string results = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{jsonResponse}\n");

            inspectionRecordAggregated = JsonConvert.DeserializeObject<List<InspectionRecordAggregated>>(results, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, SerializationBinder = new DefaultSerializationBinder() });

            return inspectionRecordAggregated;
        }
    }
}

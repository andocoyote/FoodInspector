using FoodInspectorModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FoodInspector.InspectionDataGatherer
{
    public class InspectionDataGatherer : IInspectionDataGatherer
    {
        private readonly ILogger _logger;
        private static HttpClient _httpClient;

        public InspectionDataGatherer(
            IHttpClientFactory httpClientfactory,
            ILoggerFactory loggerFactory)
        {
            _httpClient = httpClientfactory.CreateClient("InspectionDataGatherer");
            _logger = loggerFactory.CreateLogger<InspectionDataGatherer>();
        }

        public async Task<List<InspectionRecordAggregated>> QueryAllInspections()
        {
            List<InspectionRecordAggregated> inspectionRecordAggregated = null;

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

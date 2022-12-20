using FoodInspector.EstablishmentsProvider;
using FoodInspector.KeyVaultProvider;
using FoodInspector.Model;
using HttpClientTest.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Web;

namespace HttpClientTest.HttpHelpers
{
    public class CommonServiceLayerProvider : ICommonServiceLayerProvider
    {
        private string _base_uri = "https://data.kingcounty.gov/resource/";
        private string _relative_uri = "f29f-zza5.json";
        private string _startdate = "2022-01-01";
        private string _app_token = "";
        private HttpHelper _client = null;

        private readonly IKeyVaultProvider _keyVaultProvider;
        private readonly ILogger _logger;

        public CommonServiceLayerProvider(
            IKeyVaultProvider keyVaultProvider,
            ILoggerFactory loggerFactory)
        {
            _keyVaultProvider = keyVaultProvider;
            _logger = loggerFactory.CreateLogger<CommonServiceLayerProvider>();

            _app_token = _keyVaultProvider.GetAppToken().GetAwaiter().GetResult();
            _client = new HttpHelper(_base_uri, new HttpConfiguration(_app_token, "application/json"));
        }

        /// <summary>
        /// Obtains food inspection data for a set of EstablishmentModels
        /// </summary>
        /// <param name="establishmentsModels">The list of EstablishmentModels to query</param>
        /// <returns>
        /// A list of InspectionData objects containing the data for each establishment.
        /// Returns null if an exception occurs.
        /// Returns an empty list if no data is found.
        /// </returns>
        public async Task<List<InspectionData>> GetInspections(List<EstablishmentsModel> establishmentsModels)
        {
            List<InspectionData> list = new List<InspectionData>();

            try
            {
                foreach (EstablishmentsModel establishmentsModel in establishmentsModels)
                {
                    // Set the parameter values on which to search
                    InspectionDataInvocation inspectionRequest = CreateInspectionRequest(
                        establishmentsModel.Name,
                        establishmentsModel.City,
                        _startdate);

                    // Call the API to obtain the data
                    // The HttpClient does the actual calls to get the data.  CommonServiceLayerProvide just tells HttpClient what to do
                    List<InspectionData> results = await _client.DoGetAsync<List<InspectionData>>(inspectionRequest.Query, null, 3);

                    list.AddRange(results);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetInspections] Failed to query inspection data. Exception: {ex}");
                list = null;
            }

            return list;
        }

        /// <summary>
        /// Obtains food inspection data for establishments matching the name, city, and startdate
        /// </summary>
        /// <param name="name">The name of the establishment for which to query</param>
        /// <param name="city">The city of the establishment for which to query</param>
        /// <param name="startdate">The earliest date of the inspection for which to query</param>
        /// <returns>
        /// A list of InspectionData objects containing the data for each establishment.
        /// Returns null if an exception occurs.
        /// Returns an empty list if no data is found.
        /// </returns>
        public async Task<List<InspectionData>> GetInspections(string name, string city, string startdate)
        {
            List<InspectionData> list = null;

            try
            {
                // Set the parameter values on which to search
                InspectionDataInvocation inspectionRequest = CreateInspectionRequest(
                    name,
                    city,
                    startdate);

                // Call the API to obtain the data
                // The HttpClient does the actual calls to get the data.  CommonServiceLayerProvide just tells HttpClient what to do
                list = await _client.DoGetAsync<List<InspectionData>>(inspectionRequest.Query, null, 3);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetInspections] Failed to query inspection data for {name} in {city} from {startdate}. Exception: {ex}");
                list = null;
            }

            return list;
        }

        // This method creates the URI used to call the API from the query parameters
        private InspectionDataInvocation CreateInspectionRequest(string name, string city, string startdate)
        {
            // Format the query URI to contain the complete URI plus search parameters
            UriBuilder builder = new UriBuilder(_base_uri + _relative_uri);
            NameValueCollection query = HttpUtility.ParseQueryString(builder.Query);
            query["$limit"] = "50000";

            if (!string.IsNullOrEmpty(name))
            {
                query["name"] = name.ToUpper();
            }

            if (!string.IsNullOrEmpty(city))
            {
                query["city"] = city.ToUpper();
            }

            // Ex: "city in('KIRKLAND', 'REDMOND') AND inspection_date > 2020-01-01"
            query["$where"] = "inspection_date > \'" + (!string.IsNullOrEmpty(startdate) ? startdate : "2020-01-01") + "T00:00:00.000\'";

            builder.Query = query.ToString();

            // Create the request object with the parameters for which to search
            InspectionDataInvocation inspectionDataRequest = new InspectionDataInvocation
            {
                Name = name,
                City = city,
                Inspection_Date = startdate,
                Query = builder.ToString()
            };

            return inspectionDataRequest;
        }
    }
}

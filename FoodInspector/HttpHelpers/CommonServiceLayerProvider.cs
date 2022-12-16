﻿using FoodInspector.InspectionDataWriter;
using FoodInspector.KeyVaultProvider;
using HttpClientTest.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace HttpClientTest.HttpHelpers
{
    public class CommonServiceLayerProvider : ICommonServiceLayerProvider
    {
        private string _base_uri = "https://data.kingcounty.gov/resource/";
        private string _relative_uri = "f29f-zza5.json";
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

            try
            {
                _app_token = _keyVaultProvider.GetAppToken().GetAwaiter().GetResult();
                _client = new HttpHelper(_base_uri, new HttpConfiguration(_app_token, "application/json"));
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"[CommonServiceLayerProvider] An exception was caught. Exception: {ex}");
            }
        }

        public async Task<List<FoodInspector.Model.InspectionData>> GetInspections(string name, string city, string date)
        {
            try
            {
                // Set the parameter values on which to search
                InspectionDataInvocation inspectionRequest = CreateInspectionRequest(
                    name,
                    city,
                    date);

                // The HttpClient does the actual calls to get the data.  CommonServiceLayerProvide just tells HttpClient what to do
                return await _client.DoGetAsync<List<FoodInspector.Model.InspectionData>>(inspectionRequest.Query, null, 3);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to query inspection data for {name} in {city} from {date}. Exception: {ex}");
                throw;
            }
        }


        private InspectionDataInvocation CreateInspectionRequest(string name, string city, string date)
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
            query["$where"] = "inspection_date > \'" + (!string.IsNullOrEmpty(date) ? date : "2020-01-01") + "T00:00:00.000\'";

            builder.Query = query.ToString();

            // Create the request object with the parameters for which to search
            InspectionDataInvocation inspectionDataRequest = new InspectionDataInvocation
            {
                Name = name,
                City = city,
                Inspection_Date = date,
                Query = builder.ToString()
            };

            return inspectionDataRequest;
        }
    }
}

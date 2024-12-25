using Azure.AI.OpenAI;
using Azure.Identity;
using CommonFunctionality.AzureAI;
using CommonFunctionality.CosmosDbProvider;
using FoodInspectorModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Security.Policy;
using System.Text.Json;

namespace FoodInspector.Providers.AzureAIProvider
{
    public class AzureAIProvider : IAzureAIProvider
    {
        private readonly ILogger _logger;
        IOptions<AzureAIOptions> _azureAIOptions;

        public AzureAIProvider(
            IOptions<AzureAIOptions> azureAIOptions,
            ILoggerFactory loggerFactory)
        {
            _azureAIOptions = azureAIOptions;
            _logger = loggerFactory.CreateLogger<AzureAIProvider>();
        }

        public string Check(List<InspectionRecordOpenAIRequestModel> inspectionRecordOpenAIRequestModels)
        {
            string systemMessage = "You are a municipal health department worker who views food establishment (e.g. restaurant) inspection data from health inspectors in the field.\r\nYour purpose is to make recommendations of establishments to visit as a consumer and establishments to avoid based on the inspection data provided to you as JSON.\r\nBelow is a description of the JSON fields to consider when evaluating the inspection data to make your recommendations:\r\n- \"inspection_score\": any value greater that is not null, empty, or 0 indicates violations were found.  This is the sum of \"violation_points\" for the inspection identified by \"inspection_serial_num\"\r\n- \"inspection_closed_business\": a value of 'true' means that the violations were so severe, the health department immediately closed the establishment\r\n- \"violation_description\": this is a description of the requirement that was violated.  For example, \"4100 - Warewashing facilities properly installed, maintained, used;\" means the establishment did not meet this requirement\r\n- \"violation_points\": \"5\": indicates the point value of the violation.  Each violation has a 'score' value associated with it that indicates severity\r\n- \"inspection_serial_num\": the unique identifier of the inspection.  In the JSON data supplied to you, identical \"inspection_serial_num\" values indicate these entries were from a single inspection.  In other words, you want to group results by this field\r\nWhen you make recommendations of establishments to visit as a consumer and establishments to avoid, provide the following JSON fields as part of your recommendations:\r\n- \"name\"\r\n- \"inspection_date\"\r\n- \"city\"\r\n- \"inspection_score\"\r\n- \"inspection_result\"\r\n- \"inspection_closed_business\"\r\n- \"violation_description\"\r\n- \"violation_points\"\r\n- \"inspection_serial_num\"\r\nPlease recommend the 5 best places to visit and the 5 worst places that should be avoided.  If there are ties for worst and best that yield more the 5 results, recommend as many as necessary to capture all ties.";
            var OpenAIClient = new AzureOpenAIClient(new Uri(_azureAIOptions.Value.Endpoint), new DefaultAzureCredential());
            var OpenAIChatClient = OpenAIClient.GetChatClient(_azureAIOptions.Value.Deployment);

            string inpections = JsonSerializer.Serialize(inspectionRecordOpenAIRequestModels);

            // send to AI engine
            ChatCompletion completion = OpenAIChatClient.CompleteChat(
            [
                new SystemChatMessage(systemMessage),
                new UserChatMessage(inpections)
            ]);

            return completion.Content[0].Text;
        }
    }
}

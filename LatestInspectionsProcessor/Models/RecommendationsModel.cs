using System.Text.Json.Serialization;

namespace LatestInspectionsProcessor.Models
{
    public class RecommendationsModel
    {
        [JsonPropertyName("recommended")]
        public List<string>? Recommended { get; set; }

        [JsonPropertyName("unrecommended")]
        public List<string>? Unrecommended { get; set; }
    }
}

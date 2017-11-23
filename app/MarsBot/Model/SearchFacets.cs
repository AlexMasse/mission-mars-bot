using Newtonsoft.Json;

namespace MarsBot.Model
{
    public class SearchFacets
    {
        [JsonProperty("category@odata.type")]
        public string CategoryOdataType { get; set; }

        public Category[] Category { get; set; }
    }
}
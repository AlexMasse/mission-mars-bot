using Newtonsoft.Json;

namespace MarsBot.Model
{
    public class SearchResult
    {
        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }

        public SearchResultHit[] Value { get; set; }
    }
}
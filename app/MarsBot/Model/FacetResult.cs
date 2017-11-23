using Newtonsoft.Json;

namespace MarsBot.Model
{
    public class FacetResult
    {
        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }

        [JsonProperty("@search.facets")]
        public SearchFacets Facets { get; set; }

        public SearchResultHit[] Value { get; set; }
    }
}
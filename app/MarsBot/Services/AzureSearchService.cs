using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Configuration;

using MarsBot.Model;

using Newtonsoft.Json;

namespace MarsBot.Services
{
    [Serializable]
    public class AzureSearchService
    {
        private readonly string _queryString = $"https://{WebConfigurationManager.AppSettings["AzureSearchAccount"]}.search.windows.net/indexes/{WebConfigurationManager.AppSettings["AzureSearchIndex"]}/docs?api-key={WebConfigurationManager.AppSettings["AzureSearchKey"]}&api-version=2016-09-01&";

        public async Task<SearchResult> SearchByCategory(string category)
        {
            using (var httpClient = new HttpClient())
            {
                var nameQuery = $"{_queryString}$filter=category eq '{category}'";
                var response = await httpClient.GetStringAsync(nameQuery);

                return JsonConvert.DeserializeObject<SearchResult>(response);
            }
        }

        public async Task<FacetResult> FetchFacets()
        {
            using (var httpClient = new HttpClient())
            {
                var facetQuery = $"{_queryString}facet=category";
                var response = await httpClient.GetStringAsync(facetQuery);

                return JsonConvert.DeserializeObject<FacetResult>(response);
            }
        }

        public async Task<SearchResult> SearchByTitle(string title)
        {
            using (var httpClient = new HttpClient())
            {
                var nameQuery = $"{_queryString}$filter=title eq '{title}'";
                var response = await httpClient.GetStringAsync(nameQuery);
                return JsonConvert.DeserializeObject<SearchResult>(response);
            }
        }

        public async Task<SearchResult> Search(string text)
        {
            using (var httpClient = new HttpClient())
            {
                var nameQuery = $"{_queryString}search={text}";
                var response = await httpClient.GetStringAsync(nameQuery);
                return JsonConvert.DeserializeObject<SearchResult>(response);
            }
        }
    }
}
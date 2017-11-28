using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MarsBot.Services;
using MarsBot.Utils;

using Microsoft.Bot.Builder.Dialogs;

namespace MarsBot.Dialogs
{
    [Serializable]
    public class CategoryExplorerDialog : IDialog<object>
    {
        private readonly AzureSearchService _searchService = new AzureSearchService();
        private string _category;
        private string _originalText;

        public CategoryExplorerDialog(string category)
        {
            this._category = category;
        }

        public CategoryExplorerDialog(string category, string originalText)
        {
            this._category = category;
            this._originalText = originalText;
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (string.IsNullOrWhiteSpace(this._category))
            {
                var facetResult = await this._searchService.FetchFacets();
                if (facetResult.Facets.Category.Length != 0)
                {
                    var categories = facetResult.Facets.Category.Select(category => $"{category.Value} ({category.Count})").ToList();

                    PromptDialog.Choice(context, this.AfterMenuSelection, categories, "Let\'s see if I can find something in the knowledge for you. Which category is your question about?");
                }
            }
            else
            {
                var searchResult = await this._searchService.SearchByCategory(this._category);

                if (searchResult.Value.Length > 0)
                {
                    await context.PostAsync($"These are some articles I\'ve found in the knowledge base for _'{this._category}'_, click **More details** to read the full article:");
                }

                await CardUtil.ShowSearchResults(context, searchResult, $"Sorry, I could not find any results in the knowledge base for _'{this._category}'_");

                context.Done<object>(null);
            }
        }

        public virtual async Task AfterMenuSelection(IDialogContext context, IAwaitable<string> result)
        {
            this._category = await result;
            this._category = Regex.Replace(this._category, @"\s\([^)]*\)", string.Empty);

            var searchResult = await this._searchService.SearchByCategory(this._category);
            await context.PostAsync($"These are some articles I\'ve found in the knowledge base for _'{this._category}'_, click **More details** to read the full article:");

            await CardUtil.ShowSearchResults(context, searchResult, $"Sorry, I could not find any results in the knowledge base for _'{this._category}'_");

            context.Done<object>(null);
        }
    }
}
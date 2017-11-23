using System;
using System.Threading;
using System.Threading.Tasks;
using MarsBot.Services;
using MarsBot.Utils;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;

namespace MarsBot.Dialogs
{
    public class SearchScorable : ScorableBase<IActivity, string, double>
    {
        private const string Trigger = "search about ";
        private readonly AzureSearchService _searchService = new AzureSearchService();

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override double GetScore(IActivity item, string state)
        {
            return 1.0;
        }

        protected override bool HasScore(IActivity item, string state)
        {
            return !string.IsNullOrWhiteSpace(state);
        }

        protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var searchResult = await this._searchService.Search(state);

            var replyActivity = ((Activity) item).CreateReply();
            await CardUtil.ShowSearchResults(replyActivity, searchResult,
                $"I'm sorry, I did not understand '{state}'.\nType 'help' to know more about me :)");
        }

        protected override async Task<string> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item.AsMessageActivity();
            if (!string.IsNullOrWhiteSpace(message?.Text))
            {
                if (message.Text.Trim().StartsWith(Trigger, StringComparison.InvariantCultureIgnoreCase))
                {
                    return message.Text.Substring(Trigger.Length);
                }
            }

            return null;
        }
    }
}
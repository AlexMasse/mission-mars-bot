using System;
using System.Threading;
using System.Threading.Tasks;
using MarsBot.Services;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;

namespace MarsBot.Dialogs
{
    public class ShowArticleDetailsScorable : ScorableBase<IActivity, string, double>
    {
        private const string Trigger = "show me the article ";
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
            var connector = new ConnectorClient(new Uri(item.ServiceUrl));
            var reply = "Sorry, I could not find that article.";

            var searchResult = await this._searchService.SearchByTitle(state.ToString());
            if (searchResult != null && searchResult.Value.Length != 0)
            {
                reply = searchResult.Value[0].Text;
            }

            var replyActivity = ((Activity)item).CreateReply(reply);
            await connector.Conversations.ReplyToActivityAsync(replyActivity);
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
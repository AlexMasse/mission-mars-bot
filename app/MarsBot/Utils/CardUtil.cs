using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarsBot.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace MarsBot.Utils
{
    public class CardUtil
    {
        public static async Task ShowSearchResults(IDialogContext context, SearchResult searchResult, string notResultsMessage)
        {
            var reply = ((Activity)context.Activity).CreateReply();

            await ShowSearchResults(reply, searchResult, notResultsMessage);
        }

        public static async Task ShowSearchResults(Activity reply, SearchResult searchResult, string notResultsMessage)
        {
            if (searchResult.Value.Length != 0)
            {
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                var cardImages = new[] { new CardImage("https://raw.githubusercontent.com/GeekTrainer/help-desk-bot-lab/master/assets/botimages/head-smiling-medium.png") };

                foreach (var item in searchResult.Value)
                {
                    var cardButtons = new List<CardAction>();
                    var button = new CardAction()
                    {
                        Value = $"show me the article {item.Title}",
                        Type = "postBack",
                        Title = "More details"
                    };
                    cardButtons.Add(button);

                    var card = new ThumbnailCard()
                    {
                        Title = item.Title,
                        Subtitle = $"Category: {item.Category} | Search Score: {item.SearchScore}",
                        Text = item.Text.Substring(0, 50) + "...",
                        Images = cardImages,
                        Buttons = cardButtons
                    };
                    reply.Attachments.Add(card.ToAttachment());
                }

                var connector = new ConnectorClient(new Uri(reply.ServiceUrl));
                await connector.Conversations.SendToConversationAsync(reply);
            }
            else
            {
                reply.Text = notResultsMessage;
                var connector = new ConnectorClient(new Uri(reply.ServiceUrl));
                await connector.Conversations.SendToConversationAsync(reply);
            }
        }
    }
}
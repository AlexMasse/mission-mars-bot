using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AdaptiveCards;

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
                    var button = new CardAction
                    {
                        Value = $"show me the article {item.Title}",
                        Type = "postBack",
                        Title = "More details"
                    };
                    cardButtons.Add(button);

                    var card = new ThumbnailCard
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

        public static AdaptiveCard CreateTicketCard(int ticketId, string category, string severity, string description)
        {
            var card = new AdaptiveCard();

            var headerBlock = new TextBlock()
            {
                Text = $"Ticket #{ticketId}",
                Weight = TextWeight.Bolder,
                Size = TextSize.Large,
                Speak = $"<s>You've created a new Ticket #{ticketId}</s><s>We will contact you soon.</s>"
            };

            var columnsBlock = new ColumnSet()
            {
                Separation = SeparationStyle.Strong,
                Columns = new List<Column>
                {
                    new Column
                    {
                        Size = "1",
                        Items = new List<CardElement>
                        {
                            new FactSet
                            {
                                Facts = new List<AdaptiveCards.Fact>
                                {
                                    new AdaptiveCards.Fact("Severity:", severity),
                                    new AdaptiveCards.Fact("Category:", category),
                                }
                            }
                        }
                    },
                    new Column
                    {
                        Size = "auto",
                        Items = new List<CardElement>
                        {
                            new Image
                            {
                                Url =
                                    "https://raw.githubusercontent.com/GeekTrainer/help-desk-bot-lab/master/assets/botimages/head-smiling-medium.png",
                                Size = ImageSize.Small,
                                HorizontalAlignment = HorizontalAlignment.Right
                            }
                        }
                    }
                }
            };

            var descriptionBlock = new TextBlock
            {
                Text = description,
                Wrap = true
            };

            card.Body.Add(headerBlock);
            card.Body.Add(columnsBlock);
            card.Body.Add(descriptionBlock);

            return card;
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AdaptiveCards;

using MarsBot.Utils;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace MarsBot.Dialogs
{
    [Serializable]
    [LuisModel("41bda123-811e-4ee4-a2ee-0ab72b982874", "1af23f0a5fe545c5b6630dcb49750420")]
    public class RootDialog : LuisDialog<object>
    {
        private string _category;
        private string _severity;
        private string _description;

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"I'm sorry, I did not understand {result.Query}.\nType 'help' to know more about me :)");
            context.Done<object>(null);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm the help desk bot and I can help you create a ticket or explore the knowledge base.\n" +
                                    "You can tell me things like _I need to reset my password_ or _explore hardware articles_.");
            context.Done<object>(null);
        }

        [LuisIntent("SubmitTicket")]
        public async Task SubmitTicket(IDialogContext context, LuisResult result)
        {
            result.TryFindEntity("category", out var categoryEntityRecommendation);
            result.TryFindEntity("severity", out var severityEntityRecommendation);

            this._category = ((List<object>)categoryEntityRecommendation?.Resolution["values"])?[0]?.ToString();
            this._severity = ((List<object>)severityEntityRecommendation?.Resolution["values"])?[0]?.ToString();
            this._description = result.Query;

            await this.EnsureTicket(context);
        }

        [LuisIntent("ExploreKnowledgeBase")]
        public async Task ExploreCategory(IDialogContext context, LuisResult result)
        {
            result.TryFindEntity("category", out var categoryEntityRecommendation);
            var category = ((List<object>)categoryEntityRecommendation?.Resolution["values"])?[0]?.ToString();

            context.Call(new CategoryExplorerDialog(category, result.Query), this.ResumeAndEndDialogAsync);
        }

        private async Task ResumeAndEndDialogAsync(IDialogContext context, IAwaitable<object> argument)
        {
            context.Done<object>(null);
        }

        private async Task EnsureTicket(IDialogContext context)
        {
            if (this._severity == null)
            {
                var severities = new string[] { "high", "normal", "low" };
                PromptDialog.Choice(context, this.SeverityMessageReceivedAsync, severities, "Which is the severity of this problem?");
            }
            else if (this._category == null)
            {
                PromptDialog.Text(context, this.CategoryMessageReceivedAsync, "Which would be the category for this ticket (software, hardware, networking, security or other)?");
            }
            else
            {
                var text = $"Great! I'm going to create a **{this._severity}** severity ticket in the **{this._category}** category. " +
                           $"The description I will use is _\"{this._description}\"_. Can you please confirm that this information is correct?";

                PromptDialog.Confirm(context, this.IssueConfirmedMessageReceivedAsync, text);
            }
        }

        private async Task SeverityMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this._severity = await argument;
            await this.EnsureTicket(context);
        }

        private async Task CategoryMessageReceivedAsync(IDialogContext context, IAwaitable<string> argument)
        {
            this._category = await argument;
            await this.EnsureTicket(context);

        }

        private async Task IssueConfirmedMessageReceivedAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirmed = await argument;

            if (confirmed)
            {
                var api = new TicketApiClient();
                var ticketId = await api.PostTicketAsync(this._category, this._severity, this._description);

                if (ticketId != -1)
                {
                    var message = context.MakeMessage();
                    message.Attachments = new List<Attachment>
                    {
                        new Attachment
                        {
                            ContentType = "application/vnd.microsoft.card.adaptive",
                            Content = CreateCard(ticketId, this._category, this._severity, this._description)
                        }
                    };
                    await context.PostAsync(message);
                }
                else
                {
                    await context.PostAsync("Ooops! Something went wrong while I was saving your ticket. Please try again later.");
                }
            }
            else
            {
                await context.PostAsync("Ok. The ticket was not created. You can start again if you want.");
            }
            context.Done<object>(null);
        }

        private AdaptiveCard CreateCard(int ticketId, string category, string severity, string description)
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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using MarsBot.Services;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace MarsBot.Dialogs
{
    [Serializable]
    public class UserFeedbackRequestDialog : IDialog<object>
    {
        private readonly TextAnalyticsService _textAnalyticsService = new TextAnalyticsService();

        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Text(context, this.MessageReceivedAsync, "Can you please give me feedback about this experience?");
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<string> result)
        {
            var response = await result;

            var score = await this._textAnalyticsService.Sentiment(response);

            if (double.IsNaN(score))
            {
                await context.PostAsync("Ooops! Something went wrong while analyzing your answer. An IT representative agent will get in touch with you to follow up soon.");
            }
            else
            {
                string cardText;
                string cardImageUrl;

                if (score < 0.5)
                {
                    cardText = "I understand that you might be dissatisfied with my assistance. An IT representative will get in touch with you soon to help you.";
                    cardImageUrl = "https://raw.githubusercontent.com/GeekTrainer/help-desk-bot-lab/master/assets/botimages/head-sad-small.png";
                }
                else
                {
                    cardText = "Thanks for sharing your experience.";
                    cardImageUrl = "https://raw.githubusercontent.com/GeekTrainer/help-desk-bot-lab/master/assets/botimages/head-smiling-small.png";
                }

                var msg = context.MakeMessage();
                msg.Attachments = new List<Attachment>
                {
                    new HeroCard
                    {
                        Text = cardText,
                        Images = new List<CardImage>
                        {
                            new CardImage(cardImageUrl)
                        }
                    }.ToAttachment()
                };
                await context.PostAsync(msg);

                if (score < 0.5)
                {
                    PromptDialog.Confirm(context, this.EscalateWithHumanAgent, "Do you want me to escalate this with an IT representative?");
                }
                else
                {
                    context.Done<object>(null);
                }
            }
        }

        private async Task EscalateWithHumanAgent(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirmed = await argument;

            if (confirmed)
            {
                var conversationReference = context.Activity.ToConversationReference();
                var provider = Conversation.Container.Resolve<HandOff.Provider>();

                if (provider.QueueMe(conversationReference))
                {
                    var waitingPeople = provider.Pending() > 1 ? $", there are { provider.Pending() - 1 } users waiting" : string.Empty;

                    await context.PostAsync($"Connecting you to the next available human agent... please wait{waitingPeople}.");
                }

            }

            context.Done<object>(null);
        }
    }
}
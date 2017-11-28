using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;

namespace MarsBot.HandOff
{
    public class RouterScorable : ScorableBase<IActivity, ConversationReference, double>
    {
        private readonly ConversationReference _conversationReference;
        private readonly Provider _provider;
        private readonly IBotData _botData;

        public RouterScorable(IBotData botData, ConversationReference conversationReference, Provider provider)
        {
            SetField.NotNull(out this._botData, nameof(botData), botData);
            SetField.NotNull(out this._conversationReference, nameof(conversationReference), conversationReference);
            SetField.NotNull(out this._provider, nameof(provider), provider);
        }

        protected override async Task<ConversationReference> PrepareAsync(IActivity activity, CancellationToken token)
        {
            if (!(activity is Activity message) || string.IsNullOrWhiteSpace(message.Text))
            {
                return null;
            }

            // Determine if the message comes from an agent or a user
            if (this._botData.IsAgent())
            {
                return this.PrepareRouteableAgentActivity(message.Conversation.Id);
            }

            return this.PrepareRouteableUserActivity(message.Conversation.Id);
        }

        protected override bool HasScore(IActivity item, ConversationReference destination)
        {
            return destination != null;
        }

        protected override double GetScore(IActivity item, ConversationReference destination)
        {
            return 1.0;
        }

        protected override async Task PostAsync(IActivity item, ConversationReference destination, CancellationToken token)
        {
            string textToReply;
            if (destination.Conversation.Id == _conversationReference.Conversation.Id)
            {
                textToReply = "Connecting you to the next available human agent... please wait";
            }
            else
            {
                textToReply = item.AsMessageActivity().Text;
            }

            var connector = new ConnectorClient(new Uri(destination.ServiceUrl));
            var reply = destination.GetPostToUserMessage();
            reply.Text = textToReply;

            await connector.Conversations.SendToConversationAsync(reply);
        }

        private ConversationReference PrepareRouteableAgentActivity(string conversationId)
        {
            var conversation = this._provider.FindByAgentId(conversationId);

            return conversation?.User;
        }

        private ConversationReference PrepareRouteableUserActivity(string conversationId)
        {
            var conversation = this._provider.FindByConversationId(conversationId);
            if (conversation == null)
            {
                conversation = this._provider.CreateConversation(this._conversationReference);
            }

            switch (conversation.State)
            {
                case ConversationState.ConnectedToBot:
                    return null; // continue normal flow
                case ConversationState.WaitingForAgent:
                    return conversation.User;
                case ConversationState.ConnectedToAgent:
                    return conversation.Agent;
            }

            return null;
        }

        protected override Task DoneAsync(IActivity item, ConversationReference state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
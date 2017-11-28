using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Connector;

namespace MarsBot.HandOff
{
    public class Provider
    {
        public Provider()
        {
            this.Conversations = new List<Conversation>();
        }

        public IList<Conversation> Conversations { get; }

        public int Pending()
        {
            return this.Conversations.Count(c => c.State == ConversationState.WaitingForAgent);
        }

        public Conversation CreateConversation(ConversationReference conversationReference)
        {
            var newConversation = new Conversation
            {
                User = conversationReference,
                State = ConversationState.ConnectedToBot,
                Timestamp = DateTime.Now
            };

            this.Conversations.Add(newConversation);

            return newConversation;
        }

        public Conversation FindByConversationId(string userConversationId)
        {
            return this.Conversations.FirstOrDefault(c => c.User.Conversation.Id.Equals(userConversationId));
        }

        public Conversation FindByAgentId(string agentConversationId)
        {
            return this.Conversations.FirstOrDefault(c => c.Agent != null && c.Agent.Conversation.Id.Equals(agentConversationId));
        }

        public Conversation PeekConversation(ConversationReference agentReference)
        {
            var conversation = this.Conversations
                                    .Where(c => c.State == ConversationState.WaitingForAgent)
                                    .OrderByDescending(c => c.Timestamp).FirstOrDefault();

            if (conversation != null)
            {
                conversation.State = ConversationState.ConnectedToAgent;
                conversation.Agent = agentReference;
            }

            return conversation;
        }

        public bool QueueMe(ConversationReference conversationReference)
        {
            var conversation = this.FindByConversationId(conversationReference.Conversation.Id);
            if (conversation == null)
            {
                conversation = this.CreateConversation(conversationReference);
            }

            if (conversation.State == ConversationState.ConnectedToBot)
            {
                conversation.State = ConversationState.WaitingForAgent;

                return true;
            }

            return false;
        }
    }
}
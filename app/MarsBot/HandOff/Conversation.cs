﻿using System;

using Microsoft.Bot.Connector;

namespace MarsBot.HandOff
{
    public class Conversation
    {
        public DateTime Timestamp { get; set; }

        public ConversationReference User { get; set; }

        public ConversationReference Agent { get; set; }

        public ConversationState State { get; set; }
    }
}
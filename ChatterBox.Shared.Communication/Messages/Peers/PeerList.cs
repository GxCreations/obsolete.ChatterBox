﻿using System;
using System.Collections.Generic;
using ChatterBox.Shared.Communication.Messages.Interfaces;

namespace ChatterBox.Shared.Communication.Messages.Peers
{
    public sealed class PeerList : IMessageReply
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset SentDateTimeUtc { get; set; }
        public Guid ReplyFor { get; set; }

        public PeerInformation[] Peers { get; set; }
    }

}

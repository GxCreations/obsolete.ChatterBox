﻿//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using ChatterBox.Common.Communication.Messages.Interfaces;

namespace ChatterBox.Common.Communication.Messages.Registration
{
    public sealed class RegisteredReply : IMessageReply
    {
        public int Avatar { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ReplyFor { get; set; }
        public DateTimeOffset SentDateTimeUtc { get; set; }
    }
}
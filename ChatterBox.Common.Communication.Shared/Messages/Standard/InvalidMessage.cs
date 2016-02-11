//*********************************************************
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

namespace ChatterBox.Common.Communication.Messages.Standard
{
    public sealed class InvalidMessage : IMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OriginalMessage { get; set; }
        public DateTimeOffset SentDateTimeUtc { get; set; }

        public static InvalidMessage For(string request)
        {
            return new InvalidMessage
            {
                OriginalMessage = request
            };
        }
    }
}
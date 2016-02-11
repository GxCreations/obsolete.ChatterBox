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

using System.Runtime.Serialization;

namespace ChatterBox.Client.Common.Communication.Voip.Dto
{
    public sealed class OutgoingCallRequest
    {
        [DataMember]
        public string PeerUserId { get; set; }

        [DataMember]
        public bool VideoEnabled { get; set; }

    }

    public sealed class IncomingCallReject
    {
        public string Reason { get; set; }
    }

    public sealed class MicrophoneConfig
    {
        public bool Muted { get; set; }
    }


    public sealed class TraceServerConfig
    {
        [DataMember]
        public string Ip { get; set; }

        [DataMember]
        public int Port { get; set; }
  }
   public sealed class VideoConfig
   {
        public bool On { get; set; }
   }

    public sealed class VideoControlSize
    {
        [DataMember]
        public Windows.Foundation.Size Size { get; set; }
    }
}
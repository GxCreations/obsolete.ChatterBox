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

namespace ChatterBox.Client.Common.Signaling.Dto
{
    public sealed class DtoIceCandidate
    {
#if USE_WEBRTC_API
        [DataMember]
        public string Candidate { get; set; }

        [DataMember]
        public string SdpMid { get; set; }

        [DataMember]
        public ushort SdpMLineIndex { get; set; }
#elif USE_ORTC_API
        [DataMember]
        public string CandidateType { get; set; }
        [DataMember]
        public string Foundation { get; set; }
        [DataMember]
        public string InterfaceType { get; set; }
        [DataMember]
        public string Ip { get; set; }
        [DataMember]
        public ushort Port { get; set; }
        [DataMember]
        public uint Priority { get; set; }
        [DataMember]
        public string Protocol { get; set; }
        [DataMember]
        public string RelatedAddress { get; set; }
        [DataMember]
        public ushort RelatedPort { get; set; }
        [DataMember]
        public string TcpType { get; set; }
        [DataMember]
        public uint UnfreezePriority { get; set; }
#endif //USE_ORTC_API
    }

    public sealed class DtoIceCandidates
    {
        [DataMember]
        public DtoIceCandidate[] Candidates { get; set; }
    }
}

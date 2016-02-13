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

namespace ChatterBox.Common.Communication.Messages.Relay
{
    public static class RelayMessageTags
    {
        public static string IceCandidate { get; } = nameof(IceCandidate);
        public static string InstantMessage { get; } = nameof(InstantMessage);
        public static string SdpAnswer { get; } = nameof(SdpAnswer);
        public static string SdpOffer { get; } = nameof(SdpOffer);
        public static string VoipAnswer { get; } = nameof(VoipAnswer);
        public static string VoipCall { get; } = nameof(VoipCall);
        public static string VoipHangup { get; } = nameof(VoipHangup);
        public static string VoipReject { get; } = nameof(VoipReject);
    }
}
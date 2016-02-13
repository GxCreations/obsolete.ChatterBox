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
using Windows.Graphics.Display;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

#if USE_WEBRTC_API
using RtcMediaStream = webrtc_winrt_api.MediaStream;
using RtcIceCandidate = webrtc_winrt_api.RTCIceCandidate;
#elif USE_ORTC_API
using RtcMediaStream = ChatterBox.Client.Voip.Rtc.MediaStream;
using RtcIceCandidate = ortc_winrt_api.RTCIceCandidate;
#endif //USE_WEBRTC_API

#pragma warning disable 1998

namespace ChatterBox.Client.Common.Communication.Voip.States
{
    internal abstract class BaseVoipState
    {
        public VoipContext Context { get; private set; }

        public abstract VoipStateEnum VoipState { get; }

        #region IVoipChannel Members

        public virtual async Task SetForegroundProcessId(uint processId)
        {
        }

        public virtual void DisplayOrientationChanged(DisplayOrientations orientation)
        {
        }

        public virtual async Task Answer()
        {
        }

        public virtual async Task Call(OutgoingCallRequest request)
        {
        }

        public Task<VoipState> GetVoipState()
        {
            // Should never be called.  VoipContext handles it.
            throw new NotImplementedException();
        }

        public virtual async Task Hangup()
        {
        }

        public virtual async Task OnIceCandidate(RelayMessage message)
        {
        }

        public virtual async Task OnIncomingCall(RelayMessage message)
        {
        }

        public virtual async Task OnOutgoingCallAccepted(RelayMessage message)
        {
        }

        public virtual async Task OnOutgoingCallRejected(RelayMessage message)
        {
        }

        public virtual async Task OnRemoteHangup(RelayMessage message)
        {
        }

        public virtual async Task OnAnswer(RelayMessage message)
        {
        }

        public virtual async Task OnOffer(RelayMessage message)
        {
        }

        public virtual async Task Reject(IncomingCallReject reason)
        {
        }

        #endregion

        public async Task EnterState(VoipContext context)
        {
            Context = context;
            await OnEnteringState();
        }

        public async Task LeaveState()
        {
            await OnLeavingState();
            Context = null;
        }

        public virtual async Task OnEnteringState()
        {
        }

        public virtual async Task OnLeavingState()
        {
        }

        public virtual async Task SendLocalIceCandidates(RtcIceCandidate[] candidates)
        {
        }

        internal virtual async Task OnAddStream(RtcMediaStream stream)
        {
        }

        internal async Task SuspendVoipVideo()
        {
            Context.ResetRenderers();
            // Don't send RenderFormatUpdate here.  The UI is suspending
            // and may not get the message.
            if (Context.LocalStream != null)
            {
                foreach (var track in Context.LocalStream.GetVideoTracks())
                {
                    track.Suspended = true;
                }
            }
        }

        internal async Task ResumeVoipVideo()
        {
            if (Context.LocalVideoRenderer.IsRendererAlreadySetup() &&
                Context.RemoteVideoRenderer.IsRendererAlreadySetup())
            {
                return;
            }

            Context.ResetRenderers();

            // Setup remote before local as it's more important.
            if (Context.RemoteStream != null)
            {
                var tracks = Context.RemoteStream.GetVideoTracks();
#if WIN10
                var source = Context.Media.CreateMediaSource(tracks[0], VoipContext.PeerMediaStreamId);
#else
                var source = Context.Media.CreateMediaStreamSource(tracks[0], 30, VoipContext.PeerMediaStreamId);
#endif
                Context.RemoteVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source, Context.RemoteVideoControlSize);
            }
            // TODO: Delay here prevents a crash in the MF media engine when setting up the second
            //       renderer.  Investigate why this is happening.  Occurred 100% of the time.
            await Task.Delay(3000);
            if (Context.LocalStream != null)
            {
                var tracks = Context.LocalStream.GetVideoTracks();
                foreach (var track in tracks)
                {
                    track.Suspended = false;
                }

#if WIN10
                var source = Context.Media.CreateMediaSource(tracks[0], VoipContext.LocalMediaStreamId);
#else
                var source = Context.Media.CreateMediaStreamSource(tracks[0], 30, VoipContext.LocalMediaStreamId);
#endif
                Context.LocalVideoRenderer.SetupRenderer(Context.ForegroundProcessId, source, Context.LocalVideoControlSize);
            }
        }
    }
}
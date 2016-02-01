﻿using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Common.Communication.Messages.Relay;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;

namespace ChatterBox.Client.Common.Communication.Voip
{
    public interface IVoipChannel
    {
        void SetForegroundProcessId(uint processId);
        void DisplayOrientationChanged(DisplayOrientations orientation);
        void Answer();
        // Locally initiated calls
        void Call(OutgoingCallRequest request);
        VoipState GetVoipState();
        // Hangup can happen on both sides
        void Hangup();
        void OnIceCandidate(RelayMessage message);
        // Remotely initiated calls
        void OnIncomingCall(RelayMessage message);
        void OnOutgoingCallAccepted(RelayMessage message);
        void OnOutgoingCallRejected(RelayMessage message);
        void OnRemoteHangup(RelayMessage message);
        // WebRTC signaling
        void OnAnswer(RelayMessage message);
        void OnOffer(RelayMessage message);
        void Reject(IncomingCallReject reason);
        void RegisterVideoElements(MediaElement self, MediaElement peer);

        void ConfigureMicrophone(MicrophoneConfig config);
        void SyncWithNTP(long ntpTime);
        void StartTrace();
        void StopTrace();
        void SaveTrace(TraceServerConfig traceServer);

        void SuspendVoipVideo();
        void ResumeVoipVideo();
        void ConfigureVideo(VideoConfig config);
        void OnRemoteControlSize(VideoControlSize size);
        void OnLocalControlSize(VideoControlSize size);
    }
}

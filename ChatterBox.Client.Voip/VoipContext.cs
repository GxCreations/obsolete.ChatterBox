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

using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Voip.States;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Common.Communication.Messages.Relay;
using ChatterBox.Client.Voip;
using ChatterBox.Client.Voip.Utils;
using ChatterBox.Client.Voip.States.Interfaces;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Graphics.Display;
using System.Threading;
using Windows.UI.Core;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System.Threading;

#if USE_WEBRTC_API
using RtcMediaStream = webrtc_winrt_api.MediaStream;
using RtcEngine = webrtc_winrt_api.WebRTC;
using RtcMedia = webrtc_winrt_api.Media;
using RtcPeerConnection = webrtc_winrt_api.RTCPeerConnection;
using RtcIceCandidate = webrtc_winrt_api.RTCIceCandidate;
using RtcResolutionHelper = webrtc_winrt_api.ResolutionHelper;
using RtcHelper = ChatterBox.Client.Voip.Rtc.Helper;
using RtcFrameCounterHelper = webrtc_winrt_api.FrameCounterHelper;
#elif USE_ORTC_API
using RtcMediaStream = ChatterBox.Client.Voip.Rtc.MediaStream;
using RtcEngine = ChatterBox.Client.Voip.Rtc.Engine;
using RtcMedia = ChatterBox.Client.Voip.Rtc.Media;
using RtcPeerConnection = ChatterBox.Client.Voip.Rtc.RTCPeerConnection;
using RtcIceCandidate = ortc_winrt_api.RTCIceCandidate;
using RtcResolutionHelper = ChatterBox.Client.Voip.Rtc.ResolutionHelper;
using RtcHelper = ChatterBox.Client.Voip.Rtc.Helper;
using RtcFrameCounterHelper = ChatterBox.Client.Voip.Rtc.FrameCounterHelper;
#endif //USE_WEBRTC_API

using DtoMediaDevice = ChatterBox.Client.Common.Media.Dto.MediaDevice;
using DtoMediaDevices = ChatterBox.Client.Common.Media.Dto.MediaDevices;
using DtoCodecInfo = ChatterBox.Client.Common.Media.Dto.CodecInfo;
using DtoCodecInfos = ChatterBox.Client.Common.Media.Dto.CodecInfos;
using DtoMediaRatio = ChatterBox.Client.Common.Media.Dto.MediaRatio;
using DtoCaptureCapability = ChatterBox.Client.Common.Media.Dto.CaptureCapability;
using DtoCaptureCapabilities = ChatterBox.Client.Common.Media.Dto.CaptureCapabilities;
using DtoVideoCaptureFormat = ChatterBox.Client.Common.Media.Dto.VideoCaptureFormat;

#pragma warning disable 4014

namespace ChatterBox.Client.Common.Communication.Voip
{
    internal class VoipContext
    {
        private CoreDispatcher _dispatcher;
        private Func<IVideoRenderHelper> _renderResolver;
        private IHub _hub;
        private ApplicationDataContainer _localSettings;

        public const string LocalMediaStreamId = "LOCAL";
        public const string PeerMediaStreamId = "PEER";

        public VoipContext(IHub hub,
                           CoreDispatcher dispatcher,
                           Func<IVideoRenderHelper> renderResolver,
                           IVoipCoordinator coordinator)
        {
            _hub = hub;
            _dispatcher = dispatcher;
            _renderResolver = renderResolver;
            _localSettings = ApplicationData.Current.LocalSettings;
            VoipCoordinator = coordinator;

            var idleState = new VoipState_Idle();
            SwitchState(idleState).Wait();

            ResetRenderers();
        }

        #region IMediaSettingsChannel

        public DtoMediaDevices GetVideoCaptureDevices()
        {
            return Media.GetVideoCaptureDevices().ToArray().ToDto();
        }

        public DtoMediaDevices GetAudioCaptureDevices()
        {
            return DtoExtensions.ToDto(Media.GetAudioCaptureDevices().ToArray());
        }

        public DtoMediaDevices GetAudioPlayoutDevices()
        {
            return DtoExtensions.ToDto(Media.GetAudioPlayoutDevices().ToArray());
        }

        public DtoCodecInfos GetAudioCodecs()
        {
            return DtoExtensions.ToDto(RtcEngine.GetAudioCodecs().ToArray());
        }

        public DtoCodecInfos GetVideoCodecs()
        {
            return DtoExtensions.ToDto(RtcEngine.GetVideoCodecs().ToArray());
        }

        private DtoMediaDevice _videoDevice;
        public DtoMediaDevice GetVideoDevice()
        {
            return _videoDevice;
        }
        public void SetVideoDevice(DtoMediaDevice device)
        {
            _videoDevice = device;
            _localSettings.Values[MediaSettingsIds.VideoDeviceSettings] = device?.Id;
        }

        private DtoMediaDevice _audioDevice;
        public DtoMediaDevice GetAudioDevice()
        {
            return _audioDevice;
        }
        public void SetAudioDevice(DtoMediaDevice device)
        {
            _audioDevice = device;
            _localSettings.Values[MediaSettingsIds.AudioDeviceSettings] = device?.Id;
            if ((null != device) &&
                (null != Media))
            {
                Media.SelectAudioDevice(DtoExtensions.FromDto(device));
            }
        }

        private DtoCodecInfo _videoCodec;
        public DtoCodecInfo GetVideoCodec()
        {
            return _videoCodec;
        }
        public void SetVideoCodec(DtoCodecInfo codec)
        {
            _videoCodec = codec;
            _localSettings.Values[MediaSettingsIds.VideoCodecSettings] = codec?.Id;
        }

        private DtoCodecInfo _audioCodec;
        public DtoCodecInfo GetAudioCodec()
        {
            return _audioCodec;
        }
        public void SetAudioCodec(DtoCodecInfo codec)
        {
            _audioCodec = codec;
            _localSettings.Values[MediaSettingsIds.AudioCodecSettings] = codec?.Id;
        }

        private DtoMediaDevice _audioPlayoutDevice;
        public DtoMediaDevice GetAudioPlayoutDevice()
        {
            return _audioPlayoutDevice;
        }
        public void SetAudioPlayoutDevice(DtoMediaDevice device)
        {
            _audioPlayoutDevice = device;
            _localSettings.Values[MediaSettingsIds.AudioPlayoutDeviceSettings] = device?.Id;
            RtcHelper.SelectAudioPlayoutDevice(PeerConnection, Media, DtoExtensions.FromDto(device));
        }

        public DtoCaptureCapabilities GetVideoCaptureCapabilities(DtoMediaDevice device)
        {
            return GetVideoCaptureCapabilitiesAsync(device).Result;
        }

        public async Task<DtoCaptureCapabilities> GetVideoCaptureCapabilitiesAsync(DtoMediaDevice device)
        {
            var checkDevice = DtoExtensions.FromDto(device);
            var capabilities = await checkDevice.GetVideoCaptureCapabilities();
            return DtoExtensions.ToDto(capabilities.ToArray());
        }

        public void SetPreferredVideoCaptureFormat(DtoVideoCaptureFormat format)
        {
            _localSettings.Values[MediaSettingsIds.PreferredVideoCaptureWidth] = format.Width;
            _localSettings.Values[MediaSettingsIds.PreferredVideoCaptureHeight] = format.Height;
            _localSettings.Values[MediaSettingsIds.PreferredVideoCaptureFrameRate] = format.FrameRate;
        }

        public async Task InitializeMediaAsync()
        {
            // TODO: Let's remove this.
            await InitializeRTC();
        }

        public async Task<bool> RequestAccessForMediaCaptureAsync()
        {
            return await RtcEngine.RequestAccessForMediaCapture().AsTask();
        }

        public void SyncWithNTP(long ntpTime)
        {
            RtcEngine.SynNTPTime(ntpTime);
        }

        public void StartTrace()
        {
            RtcEngine.StartTracing();
            AppPerformanceCheck();
        }

        public void StopTrace()
        {
            RtcEngine.StopTracing();
            if (_appPerfTimer != null)
            {
                _appPerfTimer.Cancel();
            }
        }

        public void SaveTrace(TraceServerConfig traceServer)
        {
            RtcEngine.SaveTrace(traceServer.Ip, traceServer.Port);
        }

        public void ToggleETWStats(bool enabled)
        {
            ETWStatsEnabled = enabled;
        }

        public void ReleaseDevices()
        {
            RtcMedia.OnAppSuspending();
        }


#endregion

        /// <summary>
        /// On Win10 in a background task, WebRTC initialization has to be done
        /// when we have access to the resources.  That's inside an active
        /// voip call.
        /// This function must be called after VoipCoordinator.StartVoipTask()
        /// </summary>
        /// <returns></returns>
        public async Task InitializeRTC()
        {
            if (Media == null)
            {
                RtcEngine.Initialize(_dispatcher);
                Media = RtcMedia.CreateMedia();
                RtcMedia.SetDisplayOrientation(_displayOrientation);
                await Media.EnumerateAudioVideoCaptureDevices();
            }

            if (DisplayOrientations.None != _displayOrientation)
            {
                RtcMedia.SetDisplayOrientation(_displayOrientation);
            }

            string videoDeviceId = string.Empty;
            if (_localSettings.Values.ContainsKey(MediaSettingsIds.VideoDeviceSettings))
            {
                videoDeviceId = (string)_localSettings.Values[MediaSettingsIds.VideoDeviceSettings];
            }
            var videoDevices = Media.GetVideoCaptureDevices();
            var selectedVideoDevice = videoDevices.FirstOrDefault(d => d.Id.Equals(videoDeviceId));
            selectedVideoDevice = selectedVideoDevice ?? videoDevices.FirstOrDefault();
            if (selectedVideoDevice != null)
            {
                Media.SelectVideoDevice(selectedVideoDevice);
            }

            string audioDeviceId = string.Empty;
            if (_localSettings.Values.ContainsKey(MediaSettingsIds.AudioDeviceSettings))
            {
                audioDeviceId = (string)_localSettings.Values[MediaSettingsIds.AudioDeviceSettings];
            }
            var audioDevices = Media.GetAudioCaptureDevices();
            var selectedAudioDevice = audioDevices.FirstOrDefault(d => d.Id.Equals(audioDeviceId));
            selectedAudioDevice = selectedAudioDevice ?? audioDevices.FirstOrDefault();
            if (selectedAudioDevice != null)
            {
                Media.SelectAudioDevice(selectedAudioDevice);
            }

            string audioPlayoutDeviceId = string.Empty;
            if (_localSettings.Values.ContainsKey(MediaSettingsIds.AudioPlayoutDeviceSettings))
            {
                audioPlayoutDeviceId = (string)_localSettings.Values[MediaSettingsIds.AudioPlayoutDeviceSettings];
            }
            var audioPlayoutDevices = Media.GetAudioPlayoutDevices();
            var selectedAudioPlayoutDevice = audioPlayoutDevices.FirstOrDefault(d => d.Id.Equals(audioPlayoutDeviceId));
            selectedAudioPlayoutDevice = selectedAudioPlayoutDevice ?? audioPlayoutDevices.FirstOrDefault();
            RtcHelper.SelectAudioPlayoutDevice(PeerConnection, Media, selectedAudioPlayoutDevice);

            int videoCodecId = int.MinValue;
            if (_localSettings.Values.ContainsKey(MediaSettingsIds.VideoCodecSettings))
            {
                videoCodecId = (int)_localSettings.Values[MediaSettingsIds.VideoCodecSettings];
            }
            var videoCodecs = RtcEngine.GetVideoCodecs();
            var selectedVideoCodec = videoCodecs.FirstOrDefault(c => c.Id.Equals(videoCodecId));
            SetVideoCodec(DtoExtensions.ToDto(selectedVideoCodec ?? videoCodecs.FirstOrDefault()));

            int audioCodecId = int.MinValue;
            if (_localSettings.Values.ContainsKey(MediaSettingsIds.AudioCodecSettings))
            {
                audioCodecId = (int)_localSettings.Values[MediaSettingsIds.AudioCodecSettings];
            }
            var audioCodecs = RtcEngine.GetAudioCodecs();
            var selectedAudioCodec = audioCodecs.FirstOrDefault(c => c.Id.Equals(audioCodecId));
            SetAudioCodec(DtoExtensions.ToDto(selectedAudioCodec ?? audioCodecs.FirstOrDefault()));

            if (_localSettings.Values.ContainsKey(MediaSettingsIds.PreferredVideoCaptureWidth) &&
                _localSettings.Values.ContainsKey(MediaSettingsIds.PreferredVideoCaptureHeight) &&
                _localSettings.Values.ContainsKey(MediaSettingsIds.PreferredVideoCaptureFrameRate))
            {
                RtcEngine.SetPreferredVideoCaptureFormat((int)_localSettings.Values[MediaSettingsIds.PreferredVideoCaptureWidth],
                                                         (int)_localSettings.Values[MediaSettingsIds.PreferredVideoCaptureHeight],
                                                        (int)_localSettings.Values[MediaSettingsIds.PreferredVideoCaptureFrameRate]);
            }

            RtcResolutionHelper.ResolutionChanged += (id, width, height) =>
            {
                if (id == LocalMediaStreamId)
                {
                    LocalVideoRenderer.ResolutionChanged(width, height);
                }
                else if (id == PeerMediaStreamId)
                {
                    RemoteVideoRenderer.ResolutionChanged(width, height);
                }
            };

            RtcFrameCounterHelper.FramesPerSecondChanged += (id, frameRate) =>
            {
                if (id == LocalMediaStreamId)
                {
                    LocalVideo_FrameRateUpdate(frameRate);
                }
                else if (id == PeerMediaStreamId)
                {
                    RemoteVideo_FrameRateUpdate(frameRate);
                }
            };
        }


        private ThreadPoolTimer _appPerfTimer = null;

        private void AppPerformanceCheck()
        {

            if (_appPerfTimer != null)
            {
              _appPerfTimer.Cancel();
            }

            _appPerfTimer = ThreadPoolTimer.CreatePeriodicTimer(t=> ReportAppPerf(), TimeSpan.FromSeconds(1));
        }

        private void ReportAppPerf()
        {
            RtcEngine.UpdateCPUUsage(CPUData.GetCPUUsage());
            RtcEngine.UpdateMemUsage(MEMData.GetMEMUsage());
        }


        private void LocalVideoRenderer_RenderFormatUpdate(long swapChainHandle, uint width, uint height, uint foregroundProcessId)
        {
            _hub.OnUpdateFrameFormat(
                new FrameFormat()
                {
                    IsLocal = true,
                    SwapChainHandle = swapChainHandle,
                    Width = width,
                    Height = height,
                    ForegroundProcessId = foregroundProcessId
                });
        }

        private void LocalVideo_FrameRateUpdate(string fpsValue)
        {
            _hub.OnUpdateFrameRate(
                new FrameRate()
                {
                    IsLocal = true,
                    FPS = fpsValue
                });
        }

        private void RemoteVideoRenderer_RenderFormatUpdate(long swapChainHandle, uint width, uint height, uint foregroundProcessId)
        {
            _hub.OnUpdateFrameFormat(
                new FrameFormat()
                {
                    IsLocal = false,
                    SwapChainHandle = swapChainHandle,
                    Width = width,
                    Height = height,
                    ForegroundProcessId = foregroundProcessId
                });
        }

        private void RemoteVideo_FrameRateUpdate(string fpsValue)
        {
            _hub.OnUpdateFrameRate(
                new FrameRate()
                {
                    IsLocal = false,
                    FPS = fpsValue
                });
        }

        public RtcMedia Media { get; private set; }

        private bool _isVideoEnabled;
        public bool IsVideoEnabled
        {
            get
            {
                // TODO: lock(this) in await/async model doesn't work properly. Replace this with safer version (semaphore?)
                // NOTE: Possible issue with using semaphores is that the semaphore on the class is not re-entrent so
                //       the VoipChannel may lock with the semaphore, call the context via WithState which calls the
                //       state machine which then calls back to the Context but it's already locked. Since the semaphore
                //       is not re-entrant this could cause a deadlock.
                lock (this)
                {
                    return _isVideoEnabled;
                }
            }
            set
            {
                lock (this)
                {
                    _isVideoEnabled = value;
                    ApplyVideoConfig();
                }
            }
        }

        public IVoipCoordinator VoipCoordinator { get; set; }

        private RtcMediaStream _localStream;
        public RtcMediaStream LocalStream
        {
            get
            {
                return _localStream;
            }
            set
            {
                _localStream = value;
                ApplyMicrophoneConfig();
                ApplyVideoConfig();
            }
        }
        public RtcMediaStream RemoteStream { get; set; }

        private Timer _iceCandidateBufferTimer;
        private List<RtcIceCandidate> _bufferedIceCandidates = new List<RtcIceCandidate>();
        private SemaphoreSlim _iceBufferSemaphore = new SemaphoreSlim(1, 1);
        private async Task QueueIceCandidate(RtcIceCandidate candidate)
        {
            using (var @lock = new AutoLock(_iceBufferSemaphore))
            {
                await @lock.WaitAsync();
                _bufferedIceCandidates.Add(candidate);
                if (_iceCandidateBufferTimer == null)
                {
                    // Flush the ice candidates in 100ms.
                    _iceCandidateBufferTimer = new Timer(FlushBufferedIceCandidates, null, 100, Timeout.Infinite);
                }
            }
        }

        private async void FlushBufferedIceCandidates(object state)
        {
            using (var @lock = new AutoLock(_iceBufferSemaphore))
            {
                await @lock.WaitAsync();
                _iceCandidateBufferTimer = null;

                // Chunk in groups of 10 to not blow the size limit
                // on the storage used by the receiving side.
                while (_bufferedIceCandidates.Count > 0)
                {
                    var candidates = _bufferedIceCandidates.Take(10).ToArray();
                    _bufferedIceCandidates = _bufferedIceCandidates.Skip(10).ToList();
                    await WithState(async st => await st.SendLocalIceCandidates(candidates));
                }
            }
        }

        private bool _etwStatsEnabled = false;

        public bool ETWStatsEnabled
        {
            get { return _etwStatsEnabled; }
            set
            {
                _etwStatsEnabled = value;
                if (_peerConnection != null)
                {
                    _peerConnection.ToggleETWStats(_etwStatsEnabled);
                }
            }
        }

        private RtcPeerConnection _peerConnection { get; set; }
        public RtcPeerConnection PeerConnection
        {
            get { return _peerConnection; }
            set
            {
                _peerConnection = value;
                if (_peerConnection != null)
                {
                    // Register to the events from the peer connection.
                    // We'll forward them to the state.
                    _peerConnection.OnIceCandidate += evt =>
                    {
                        if (evt.Candidate != null)
                        {
                            var task = QueueIceCandidate(evt.Candidate);
                        }
                    };

                    if (_hub.IsAppInsightsEnabled)
                    {
                        _hub.InitialiazeStatsManager(_peerConnection);
                        _hub.ToggleStatsManagerConnectionState(true);
                    }

                    _peerConnection.ToggleETWStats(ETWStatsEnabled);

                    _peerConnection.OnAddStream += evt =>
                    {
                        if (evt.Stream != null)
                        {
                            Task.Run(async () =>
                            {
                                await WithState(async st => await st.OnAddStream(evt.Stream));
                            });
                        }
                    };
                }
                else
                {
                    if (_hub.IsAppInsightsEnabled)
                    {
                        _hub.ToggleStatsManagerConnectionState(false);
                    }
                }
            }
        }

        public CoreDispatcher Dispatcher { get { return _dispatcher; } }
        public string PeerId { get; set; }
        private BaseVoipState State { get; set; }

        internal VoipState GetVoipState()
        {
            return new VoipState
            {
                PeerId = PeerId,
                HasPeerConnection = PeerConnection != null,
                State = State.VoipState,
                IsVideoEnabled = IsVideoEnabled
            };
        }

        public void SendToPeer(string tag, string payload)
        {
            _hub.Relay(new RelayMessage
            {
                FromUserId = RegistrationSettings.UserId,
                ToUserId = PeerId,
                Tag = tag,
                Payload = payload
            });
        }

        public async Task SwitchState(BaseVoipState newState)
        {
            Debug.WriteLine($"VoipContext.SwitchState {State?.GetType().Name} -> {newState.GetType().Name}");
            if (State != null)
            {
                await State.LeaveState();
            }
            State = newState;
            await State.EnterState(this);

            Task.Run(() => _hub.OnVoipState(GetVoipState()));
        }


        // Semaphore used to make sure only one call
        // is executed at any given time.
        private readonly SemaphoreSlim _sem = new SemaphoreSlim(1, 1);

        public async Task WithState(Func<BaseVoipState, Task> fn)
        {
            using (var @lock = new AutoLock(_sem))
            {
                await @lock.WaitAsync();

                try
                {
                    await fn(State);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
        public async Task WithContextAction(Action<VoipContext> fn)
        {
            using (var @lock = new AutoLock(_sem))
            {
                await @lock.WaitAsync();

                try
                {
                    fn(this);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public async Task WithContextActionAsync(Func<VoipContext, Task> fn)
        {
            using (var @lock = new AutoLock(_sem))
            {
                await @lock.WaitAsync();

                try
                {
                    await fn(this);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public async Task<TResult> WithContextFunc<TResult>(Func<VoipContext, TResult> fn)
        {
            using (var @lock = new AutoLock(_sem))
            {
                await @lock.WaitAsync();

                try
                {
                    return fn(this);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return default(TResult);
        }
        public async Task<TResult> WithContextFuncAsync<TResult>(Func<VoipContext, Task<TResult>> fn)
        {
            using (var @lock = new AutoLock(_sem))
            {
                await @lock.WaitAsync();

                try
                {
                    return await fn(this);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return default(TResult);
        }

        private UInt32 _foregroundProcessId;
        public UInt32 ForegroundProcessId
        {
            get
            {
                lock (this)
                {
                    return _foregroundProcessId;
                }
            }
            set
            {
                lock (this)
                {
                    _foregroundProcessId = value;
                }
            }
        }

        private DisplayOrientations _displayOrientation = DisplayOrientations.None;
        public DisplayOrientations DisplayOrientation
        {
            get
            {
                lock (this)
                {
                    return _displayOrientation;
                }
            }
            set
            {
                lock (this)
                {
                    _displayOrientation = value;
                    if (Media != null)
                    {
                        RtcMedia.SetDisplayOrientation(_displayOrientation);
                    }
                }
            }
        }

        private bool _microphoneMuted;
        public bool MicrophoneMuted
        {
            get
            {
                lock (this)
                {
                    return _microphoneMuted;
                }
            }
            set
            {
                lock (this)
                {
                    _microphoneMuted = value;
                    ApplyMicrophoneConfig();
                }
            }
        }

        public IVideoRenderHelper LocalVideoRenderer { get; private set; }
        public IVideoRenderHelper RemoteVideoRenderer { get; private set; }
        public Windows.Foundation.Size LocalVideoControlSize { get; set; }
        public Windows.Foundation.Size RemoteVideoControlSize { get; set; }

        public void ResetRenderers()
        {
            if (LocalVideoRenderer != null)
            {
                LocalVideoRenderer.Teardown();
            }
            if (RemoteVideoRenderer != null)
            {
                RemoteVideoRenderer.Teardown();
            }

            LocalVideoRenderer = _renderResolver();
            RemoteVideoRenderer = _renderResolver();

            LocalVideoRenderer.RenderFormatUpdate += LocalVideoRenderer_RenderFormatUpdate;
            RemoteVideoRenderer.RenderFormatUpdate += RemoteVideoRenderer_RenderFormatUpdate;
            GC.Collect();
        }

        private void ApplyMicrophoneConfig()
        {
            if (LocalStream != null)
            {
                foreach (var audioTrack in LocalStream.GetAudioTracks())
                {
                    audioTrack.Enabled = !_microphoneMuted;
                }
            }
        }

        private void ApplyVideoConfig()
        {
            if (LocalStream != null)
            {
                foreach (var videoTrack in LocalStream.GetVideoTracks())
                {
                    videoTrack.Enabled = _isVideoEnabled;
                }
            }
        }

        private DateTimeOffset _callStartDateTime;

        public void TrackCallStarted()
        {
            _hub.IsAppInsightsEnabled = SignalingSettings.AppInsightsEnabled;
            if (!_hub.IsAppInsightsEnabled)
            {
                return;
            }
            _callStartDateTime = DateTimeOffset.Now;
            var currentConnection = NetworkInformation.GetInternetConnectionProfile();
            string connType;
            switch (currentConnection.NetworkAdapter.IanaInterfaceType)
            {
                case 6:
                    connType = "Cable";
                    break;
                case 71:
                    connType = "WiFi";
                    break;
                case 243:
                    connType = "Mobile";
                    break;
                default:
                    connType = "Unknown";
                    break;
            }
            var properties = new Dictionary<string, string> { { "Connection Type", connType } };
            _hub.TrackStatsManagerEvent("CallStarted", properties);
            // start call watch to count duration for tracking as request
            _hub.StartStatsManagerCallWatch();
        }

        public void TrackCallEnded()
        {
            if (!_hub.IsAppInsightsEnabled)
            {
                return;
            }
            // log call duration as CallEnded event property
            string duration = DateTimeOffset.Now.Subtract(_callStartDateTime).Duration().ToString(@"hh\:mm\:ss");
            var properties = new Dictionary<string, string> { { "Call Duration", duration } };
            _hub.TrackStatsManagerEvent("CallEnded", properties);

            // stop call watch, so the duration will be calculated and tracked as request
            _hub.StopStatsManagerCallWatch();
        }
    }
}

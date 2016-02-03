
#if USE_ORTC_API

using System.Collections.Generic;
//using Windows.Storage;
using Windows.UI.Core;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
using System;
using Windows.Graphics.Display;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using System.Threading.Tasks;
using Windows.Media.Capture;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ChatterBox.Client.Voip.Rtc
{
    //using LogLevel = webrtc_winrt_api.LogLevel;
    using DtoCaptureCapability = Common.Media.Dto.CaptureCapability;
    using DtoMediaRatio = Common.Media.Dto.MediaRatio;
    using DtoCaptureCapabilities = Common.Media.Dto.CaptureCapabilities;
    using RtcHelper = ChatterBox.Client.Voip.Rtc.Helper;
    using RtcIceCandidate = ortc_winrt_api.RTCIceCandidate;
    using RtcSender = ortc_winrt_api.RTCRtpSender;
    using RtcReceiver = ortc_winrt_api.RTCRtpReceiver;
    using RtcCodecCapability = ortc_winrt_api.RTCRtpCodecCapability;
    using RtcOrtc = ortc_winrt_api.Ortc;
    using RtcOrtcWithDispatcher = ortc_winrt_api.OrtcWithDispatcher;
    using RtcLog = ortc_winrt_api.Log;
    using RtcLogger = ortc_winrt_api.Logger;
    using RtcMediaDevices = ortc_winrt_api.MediaDevices;
    using RtcMediaDeviceInfo = ortc_winrt_api.MediaDeviceInfo;
    using RtcMediaDeviceKind = ortc_winrt_api.MediaDeviceKind;
    using RtcConstraints = ortc_winrt_api.Constraints;
    using RtcMediaTrackConstraints = ortc_winrt_api.MediaTrackConstraints;
    using RtcMediaTrackConstraintSet = ortc_winrt_api.MediaTrackConstraintSet;
    using RtcConstrainString = ortc_winrt_api.ConstrainString;
    using RtcConstrainStringParameters = ortc_winrt_api.ConstrainStringParameters;
    using RtcStringOrStringList = ortc_winrt_api.StringOrStringList;
    using RtcMediaStreamTrack = ortc_winrt_api.MediaStreamTrack;
    using RtcIceGatherer = ortc_winrt_api.RTCIceGatherer;
    using RtcIceTransport = ortc_winrt_api.RTCIceTransport;
    using RtcDtlsTransport = ortc_winrt_api.RTCDtlsTransport;
    using RtcDtlsParameters = ortc_winrt_api.RTCDtlsParameters;
    using RtcIceGatherOptions = ortc_winrt_api.RTCIceGatherOptions;
    using RtcIceServer = ortc_winrt_api.RTCIceServer;
    using RtcCertificate = ortc_winrt_api.RTCCertificate;
    using RtcIceGathererCandidateEvent = ortc_winrt_api.RTCIceGathererCandidateEvent;
    using RtcIceGathererCandidateCompleteEvent = ortc_winrt_api.RTCIceGathererCandidateCompleteEvent;
    using RtcIceParameters = ortc_winrt_api.RTCIceParameters;
    using RtcRtpCapabilities = ortc_winrt_api.RTCRtpCapabilities;
    using RtcIceRole = ortc_winrt_api.RTCIceRole;
    using RtcIceTypes = ortc_winrt_api.RTCIceTypes;

    internal delegate void OnMediaCaptureDeviceFoundDelegate(MediaDevice param);
    internal delegate void RTCPeerConnectionIceEventDelegate(RTCPeerConnectionIceEvent param);
    internal delegate void MediaStreamEventEventDelegate(MediaStreamEvent param0);
    internal delegate void RTCStatsReportsReadyEventDelegate(RTCStatsReportsReadyEvent param0);
    internal delegate void ResolutionChangedEventHandler(string id, uint width, uint height);
    internal delegate void RTCPeerConnectionHealthStatsDelegate(RTCPeerConnectionHealthStats param);
    internal delegate void StepEventHandler();

    //using EventDelegate = webrtc_winrt_api.EventDelegate;
    //using RTCDataChannelMessageEventDelegate = webrtc_winrt_api.RTCDataChannelMessageEventDelegate;
    //using RTCDataChannelEventDelegate = webrtc_winrt_api.RTCDataChannelEventDelegate;

    internal sealed class Engine
    {
        private static Engine _singleton;

        private string _tracingHost = null;
        private UInt16 _tracingPort = 0;

        private static Engine Singleton { get { if (null == _singleton) _singleton = new Engine(); return _singleton; } }

        //public static void DisableLogging() { }
        //public static void EnableLogging(LogLevel level) { }
        public static IList<CodecInfo> GetAudioCodecs() { return RtcHelper.GetCodecs("audio"); }

        //public static double GetCPUUsage() { return 0.0; }
        //public static long GetMemUsage() { return 0; }
        public static IList<CodecInfo> GetVideoCodecs() { return RtcHelper.GetCodecs("video"); }

        public static void Initialize(CoreDispatcher dispatcher)
        {
            RtcOrtcWithDispatcher.Setup(dispatcher);
        }

        //public static bool IsTracing() { return false; }
        //public static string LogFileName() { return null; }
        //public static StorageFolder LogFolder() { return null; }
        public static IAsyncOperation<bool> RequestAccessForMediaCapture() { return null; }
        //public static bool SaveTrace(string filename) { return false; }
        public static bool SaveTrace(string host, int port)
        {
            Singleton._tracingHost = host;
            Singleton._tracingPort = (UInt16)port;
            return true;
        }

        public static void SetPreferredVideoCaptureFormat(int frame_width, int frame_height, int fps) { }
        public static void StartTracing()
        {
            var port = Singleton._tracingPort;
            if (0 == port) port = 59999;

            string host = Singleton._tracingHost;

            RtcLogger.SetLogLevel(RtcLog.Level.Trace);
            RtcLogger.SetLogLevel(RtcLog.Component.ZsLib, RtcLog.Level.Trace);
            RtcLogger.SetLogLevel(RtcLog.Component.Services, RtcLog.Level.Trace);
            RtcLogger.SetLogLevel(RtcLog.Component.ServicesHttp, RtcLog.Level.Trace);
            RtcLogger.SetLogLevel(RtcLog.Component.OrtcLib, RtcLog.Level.Trace);

            if (String.IsNullOrEmpty(host))
            {
                RtcLogger.InstallOutgoingTelnetLogger(host + ":" + port.ToString(), true, null);
            }
            else {
                RtcLogger.UninstallOutgoingTelnetLogger();
            }

            RtcLogger.InstallTelnetLogger(port, 60, true);
        }

        public static void StopTracing()
        {
            RtcLogger.UninstallOutgoingTelnetLogger();
            RtcLogger.UninstallTelnetLogger();
        }

        public static void SynNTPTime(long current_ntp_time) { }
        public static void UpdateCPUUsage(double cpu_usage) { }
        public static void UpdateMemUsage(long mem_usage) { }
    }

    internal sealed class Media
    {
        private static Media _singleton;

        private RtcMediaDevices _media;

        IList<RtcMediaDeviceInfo> _audioCaptureDevices = new List<RtcMediaDeviceInfo>();
        IList<RtcMediaDeviceInfo> _audioPlaybackDevices = new List<RtcMediaDeviceInfo>();
        IList<RtcMediaDeviceInfo> _videoDevices = new List<RtcMediaDeviceInfo>();

        private MediaDevice _audioCaptureDevice;
        private MediaDevice _audioPlaybackDevice;
        private MediaDevice _videoDevice;

        public event OnMediaCaptureDeviceFoundDelegate OnAudioCaptureDeviceFound;
        public event OnMediaCaptureDeviceFoundDelegate OnVideoCaptureDeviceFound;

        private static Media Singleton { get { if (null == _singleton) _singleton = new Media(); return _singleton; } }

        public static Media CreateMedia()
        {
            var media = new Media();
            media._media = new RtcMediaDevices();
            media._media.OnDeviceChange += media.Media_OnDeviceChange;
            return media;
        }

        private void Media_OnDeviceChange()
        {
            var asyncOp = RtcMediaDevices.EnumerateDevices();
            asyncOp.Completed = (op, state) => {
                var devices = op.GetResults();
                List<RtcMediaDeviceInfo> audioCaptureEventList;
                List<RtcMediaDeviceInfo> audioCaptureReplacementList;
                RtcHelper.CalculateDeltas(RtcMediaDeviceKind.AudioInput, _audioCaptureDevices, devices, out audioCaptureEventList, out audioCaptureReplacementList);

                List<RtcMediaDeviceInfo> audioPlaybackEventList;
                List<RtcMediaDeviceInfo> audioPlaybackReplacementList;
                RtcHelper.CalculateDeltas(RtcMediaDeviceKind.AudioInput, _audioCaptureDevices, devices, out audioPlaybackEventList, out audioPlaybackReplacementList);

                List<RtcMediaDeviceInfo> videoEventList;
                List<RtcMediaDeviceInfo> videoReplacementList;
                RtcHelper.CalculateDeltas(RtcMediaDeviceKind.Video, _videoDevices, devices, out videoEventList, out videoReplacementList);

                foreach (var info in audioCaptureEventList)
                {
                    OnAudioCaptureDeviceFound(new MediaDevice(info.DeviceID, info.Label));
                }
                foreach (var info in videoEventList)
                {
                    OnVideoCaptureDeviceFound(new MediaDevice(info.DeviceID, info.Label));
                }

                _audioCaptureDevices = audioCaptureReplacementList;
                _audioPlaybackDevices = audioPlaybackReplacementList;
                _videoDevices = videoReplacementList;
            };
            throw new NotImplementedException();
        }

        //public static IAsyncOperation<Media> CreateMediaAsync() { return null; }
        public static void OnAppSuspending()
        {
#warning TODO IMPLEMENT OnAppSuspending
        }

        public static void SetDisplayOrientation(DisplayOrientations display_orientation)
        {
#warning TODO IMPLEMENT SetDisplayOrientation
        }

        public IMediaSource CreateMediaSource(MediaVideoTrack track, string id)
        {
#warning TODO IMPLEMENT CreateMediaSource
            return null;
        }

        //public IMediaSource CreateMediaStreamSource(MediaVideoTrack track, uint framerate, string id) { return null; }
        public IAsyncOperation<bool> EnumerateAudioVideoCaptureDevices() {
            return Task.Run<bool>(async () =>
            {
                var devices = await RtcMediaDevices.EnumerateDevices();

                var audioCaptureList = RtcHelper.Filter(RtcMediaDeviceKind.AudioInput, devices);
                var audioPlaybackList = RtcHelper.Filter(RtcMediaDeviceKind.AudioOutput, devices);
                var videoList = RtcHelper.Filter(RtcMediaDeviceKind.Video, devices);

                _audioCaptureDevices = audioCaptureList;
                _audioPlaybackDevices = audioPlaybackList;
                _videoDevices = videoList;

                return devices.Count > 0;
            }).AsAsyncOperation();
        }


        public IList<MediaDevice> GetAudioCaptureDevices() { return RtcHelper.ToMediaDevices(_audioCaptureDevices); }
        public IList<MediaDevice> GetAudioPlayoutDevices() { return RtcHelper.ToMediaDevices(_audioPlaybackDevices); }

        public IAsyncOperation<MediaStream> GetUserMedia(RTCMediaStreamConstraints mediaStreamConstraints)
        {
            return Task.Run<MediaStream>(async () =>
            {
                var constraints = RtcHelper.MakeConstraints(mediaStreamConstraints.audioEnabled, null, RtcMediaDeviceKind.AudioInput, _audioCaptureDevice);
                constraints = RtcHelper.MakeConstraints(mediaStreamConstraints.audioEnabled, constraints, RtcMediaDeviceKind.AudioOutput, _audioPlaybackDevice);
                constraints = RtcHelper.MakeConstraints(mediaStreamConstraints.videoEnabled, constraints, RtcMediaDeviceKind.Video, _videoDevice);

                if (null == constraints) { return new MediaStream(); }

                var tracks = await RtcMediaDevices.GetUserMedia(constraints);

                var audioTracks = RtcHelper.InsertAudioIfValid(mediaStreamConstraints.audioEnabled, null, tracks, _audioCaptureDevice);
                var videoTracks = RtcHelper.InsertVideoIfValid(mediaStreamConstraints.videoEnabled, null, tracks, _videoDevice);

                return new MediaStream(audioTracks, videoTracks);
            }).AsAsyncOperation();
        }
        public IList<MediaDevice> GetVideoCaptureDevices() { return RtcHelper.ToMediaDevices(_videoDevices); }
        public bool SelectAudioDevice(MediaDevice device) { _audioCaptureDevice = device; return true; }
        public bool SelectAudioPlayoutDevice(MediaDevice device)
        {
#warning TODO APPLY CONSTRAINTS TO THE OUTGOING TRACK
            _audioPlaybackDevice = device;
            return true;
        }
        public void SelectVideoDevice(MediaDevice device) { _audioCaptureDevice = device; }
    }

    internal sealed class RTCPeerConnection
    {
        private bool _closed = false;
        private RtcIceGatherer _iceGatherer;
        private RtcIceTransport _iceTransport;
        private RtcDtlsTransport _dtlsTransport;
        private TaskCompletionSource<RTCSessionDescription> _capabilitiesTcs;
        private TaskCompletionSource<RTCSessionDescription> _capabilitiesFinalTcs;
        private TaskCompletionSource<RTCSessionDescription> _remoteCapabilitiesTcs;
        private RtcIceRole _iceRole = RtcIceRole.Controlling;
        private RTCSessionDescription _localCapabilities;
        private RTCSessionDescription _localCapabilitiesFinal;
        private RTCSessionDescription _remoteCapabilities;
        private MediaStream _localStream;
        private MediaStream _remoteStream;
        private RtcSender _audioSender;
        private RtcSender _videoSender;
        private RtcReceiver _audioReceiver;
        private RtcReceiver _videoReceiver;

        private bool _installedIceEvents;

        private EventRegistrationTokenTable<RTCPeerConnectionIceEventDelegate> _onIceCandidate = new EventRegistrationTokenTable<RTCPeerConnectionIceEventDelegate>();

        private event StepEventHandler OnStep;

        public RTCPeerConnection(RTCConfiguration configuration)
        {
            _installedIceEvents = false;

            var options = RtcHelper.ToGatherOptions(configuration);
            _iceGatherer = new RtcIceGatherer(options);
            _iceTransport = new RtcIceTransport(_iceGatherer);

            OnStep += RTCPeerConnection_OnStep;

            RtcCertificate.GenerateCertificate("").AsTask<RtcCertificate>().ContinueWith((cert) =>
            {
                _dtlsTransport = new RtcDtlsTransport(_iceTransport, cert.Result);
                if (_closed) _dtlsTransport.Stop();
                Wake();
            });
        }

        private void Wake()
        {
            Task.Run(() => { OnStep(); });
        }

        private void RTCPeerConnection_OnStep()
        {
            if (_closed)
            {
                Close();
                return;
            }
            if (null == _dtlsTransport) return;
            StepGetCapabilities();
            StepSetupReceiver();
            StepSetupSender();
        }

        private void StepGetCapabilities()
        {
            if (null == _capabilitiesTcs) return;

            var localParams = _iceGatherer.GetLocalParameters();
            var audioSenderCaps = RtcSender.GetCapabilities("audio");
            var videoSenderCaps = RtcSender.GetCapabilities("audio");
            var audioReceiverCaps = RtcReceiver.GetCapabilities("audio");
            var videoReceiverCaps = RtcReceiver.GetCapabilities("audio");
            var dtlsParameters = _dtlsTransport.GetLocalParameters();
            bool hasAudio = false;
            bool hasVideo = false;

            if (null != _localStream)
            {
                var audioTracks = _localStream.GetAudioTracks();
                if (null != audioTracks) { hasAudio = (audioTracks.Count > 0); }
                var videoTracks = _localStream.GetVideoTracks();
                if (null != videoTracks) { hasVideo = (videoTracks.Count > 0); }
            }
            if (null != _remoteCapabilities)
            {
                hasAudio = _remoteCapabilities.Description.HasAudio;
                hasVideo = _remoteCapabilities.Description.HasVideo;
            }

            var caps = new RTCSessionDescription(new RtcDescription(hasAudio, hasVideo, _iceRole, localParams, audioSenderCaps, videoSenderCaps, audioReceiverCaps, videoReceiverCaps, dtlsParameters));
            _localCapabilities = caps;

            _capabilitiesTcs.SetResult(_localCapabilities);
            _capabilitiesTcs = null;
        }

        private void StepSetupReceiver()
        {
            if (null == _capabilitiesFinalTcs) return;

            MediaAudioTrack incomingAudioTrack = null;
            MediaVideoTrack incomingVideoTrack = null;

            bool hasAudio = _localCapabilitiesFinal.Description.HasAudio;
            bool hasVideo = _localCapabilitiesFinal.Description.HasVideo;
            if (null != _remoteCapabilities)
            {
                hasAudio = hasAudio && _remoteCapabilities.Description.HasAudio;
                hasVideo = hasVideo && _remoteCapabilities.Description.HasVideo;
            }

            if (hasAudio)
            {
                _audioReceiver = new RtcReceiver(_dtlsTransport);
#warning TODO START RECEIVER
                incomingAudioTrack = new MediaAudioTrack(_audioReceiver.Track);
            }
            if (hasVideo)
            {
#warning TODO START RECEIVER
                _videoReceiver = new RtcReceiver(_dtlsTransport);
                incomingVideoTrack = new MediaVideoTrack(_videoReceiver.Track);
            }

            List<MediaAudioTrack> incomingAudioTracks = null;
            List<MediaVideoTrack> incomingVideoTracks = null;
            if (null != incomingAudioTrack)
            {
                incomingAudioTracks = new List<MediaAudioTrack>();
                incomingAudioTracks.Add(incomingAudioTrack);
            }
            if (null != incomingVideoTrack)
            {
                incomingVideoTracks = new List<MediaVideoTrack>();
                incomingVideoTracks.Add(incomingVideoTrack);
            }

            var mediaStream = new MediaStream(incomingAudioTracks, incomingVideoTracks);
            _remoteStream = mediaStream;
            var evt = new MediaStreamEvent();
            evt.Stream = _remoteStream;
            Task.Run(() => { OnAddStream(evt); });

            _capabilitiesFinalTcs.SetResult(_localCapabilitiesFinal);
            _capabilitiesFinalTcs = null;
        }

        private void StepSetupSender()
        {
            if (null == _remoteCapabilitiesTcs) return;
            if (null == _localStream) return;
            if (null == _localCapabilitiesFinal) return;
            if (null == _remoteCapabilities) return;

            _iceTransport.Start(_iceGatherer, _remoteCapabilities.Description.IceParameters, _remoteCapabilities.Description.IceRole);
            _dtlsTransport.Start(_remoteCapabilities.Description.DtlsParameters);

            if (_localCapabilitiesFinal.Description.HasAudio)
            {
                if (null == _audioSender)
                {
                    var tracks = _localStream.GetAudioTracks();
                    MediaAudioTrack mediaTrack = null;
                    RtcMediaStreamTrack track = null;
                    if (null != tracks) { if (tracks.Count > 0) { mediaTrack = tracks.First(); } }
                    if (null != mediaTrack) track = mediaTrack.Track;

                    if (null != track)
                    {
                        _audioSender = new RtcSender(track, _dtlsTransport);
#warning START SENDER
                    }
                }
            }
            if (_localCapabilitiesFinal.Description.HasVideo)
            {
                if (null == _videoSender)
                {
                    var tracks = _localStream.GetVideoTracks();
                    MediaVideoTrack mediaTrack = null;
                    RtcMediaStreamTrack track = null;
                    if (null != tracks) { if (tracks.Count > 0) { mediaTrack = tracks.First(); } }
                    if (null != mediaTrack) track = mediaTrack.Track;

                    if (null != track)
                    {
                        _videoSender = new RtcSender(track, _dtlsTransport);
#warning START SENDER
                    }
                }
            }
            _remoteCapabilitiesTcs.SetResult(_remoteCapabilities);
            _remoteCapabilitiesTcs = null;
        }

        //public RTCIceConnectionState IceConnectionState { get; }
        //public RTCIceGatheringState IceGatheringState { get; }
        //public RTCSessionDescription LocalDescription { get; }
        //public RTCSessionDescription RemoteDescription { get; }
        //public RTCSignalingState SignalingState { get; }


        public event MediaStreamEventEventDelegate OnAddStream;
        public event RTCPeerConnectionHealthStatsDelegate OnConnectionHealthStats;
        //public event RTCDataChannelEventDelegate OnDataChannel;
        public event RTCPeerConnectionIceEventDelegate OnIceCandidate
        {
            add
            {
                if (!_installedIceEvents)
                {
                    _iceGatherer.OnICEGathererLocalCandidate += IceGatherer_OnICEGathererLocalCandidate;
                    _iceGatherer.OnICEGathererCandidateComplete += IceGatherer_OnICEGathererCandidateComplete;
                    _iceGatherer.OnICEGathererLocalCandidateGone += IceGatherer_OnICEGathererLocalCandidateGone;
                }
                return _onIceCandidate.AddEventHandler(value);
            }
            remove
            {
                _onIceCandidate.RemoveEventHandler(value);
            }
        }

        private void IceGatherer_OnICEGathererLocalCandidate(RtcIceGathererCandidateEvent evt)
        {
            RTCPeerConnectionIceEvent wrapperEvt = new RTCPeerConnectionIceEvent();
            wrapperEvt.Candidate = evt.Candidate;
            _onIceCandidate.InvocationList.Invoke(wrapperEvt);
        }

        private void IceGatherer_OnICEGathererCandidateComplete(RtcIceGathererCandidateCompleteEvent evt)
        {
#warning TODO IMPROVE ICE WIRING
            //throw new NotImplementedException();
        }

        private void IceGatherer_OnICEGathererLocalCandidateGone(RtcIceGathererCandidateEvent evt)
        {
            //throw new NotImplementedException();
#warning TODO IMPROVE ICE WIRING
        }


        //public event RTCPeerConnectionIceStateChangeEventDelegate OnIceConnectionChange;
        //public event EventDelegate OnNegotiationNeeded;
        //public event MediaStreamEventEventDelegate OnRemoveStream;
        public event RTCStatsReportsReadyEventDelegate OnRTCStatsReportsReady;
        //public event EventDelegate OnSignalingStateChange;

        public IAsyncAction AddIceCandidate(RtcIceCandidate candidate)
        {
            return Task.Run(() => { _iceTransport.AddRemoteCandidate(candidate); }).AsAsyncAction();
        }

        public void AddStream(MediaStream stream)
        {
            _localStream = stream;
            Wake();
        }

        public void Close()
        {
            _iceGatherer.Close();
            _iceTransport.Stop();
            if (null != _dtlsTransport) _dtlsTransport.Stop();
            if (null != _capabilitiesTcs)
            {
                _capabilitiesTcs.SetResult(null);
                _capabilitiesTcs = null;
            }
            if (null != _capabilitiesFinalTcs)
            {
                _capabilitiesFinalTcs.SetResult(null);
                _capabilitiesFinalTcs = null;
            }
            if (null != _remoteCapabilitiesTcs)
            {
                _remoteCapabilitiesTcs.SetResult(null);
                _remoteCapabilitiesTcs = null;
            }
            if (null != _remoteStream) {_remoteStream.Stop();}
            if (null != _audioSender) { _audioSender.Stop(); }
            if (null != _videoSender) { _videoSender.Stop(); }
            if (null != _audioReceiver) { _audioReceiver.Stop(); }
            if (null != _videoReceiver) { _videoReceiver.Stop(); }
        }

        public IAsyncOperation<RTCSessionDescription> CreateAnswer()
        {
            _iceRole = RtcIceRole.Controlled;
            _capabilitiesTcs = new TaskCompletionSource<RTCSessionDescription>();
            Wake();
            return _capabilitiesTcs.Task.AsAsyncOperation();
        }

        //public RTCDataChannel CreateDataChannel(string label, RTCDataChannelInit init) { return null; }
        public IAsyncOperation<RTCSessionDescription> CreateOffer()
        {
            _iceRole = RtcIceRole.Controlling;
            _capabilitiesTcs = new TaskCompletionSource<RTCSessionDescription>();
            Wake();
            return _capabilitiesTcs.Task.AsAsyncOperation();
        }
        //public RTCConfiguration GetConfiguration() { return null; }
        //public IList<MediaStream> GetLocalStreams() { return null; }
        //public IList<MediaStream> GetRemoteStreams() { return null; }
        //public MediaStream GetStreamById(string streamId) { return null; }
        //public void RemoveStream(MediaStream stream) { }
        public IAsyncAction SetLocalDescription(RTCSessionDescription description)
        {
            _localCapabilitiesFinal = description;
            _capabilitiesFinalTcs = new TaskCompletionSource<RTCSessionDescription>();
            Wake();
            return _capabilitiesFinalTcs.Task.AsAsyncAction();
        }
        public IAsyncAction SetRemoteDescription(RTCSessionDescription description)
        {
            _remoteCapabilities = description;
            _remoteCapabilitiesTcs = new TaskCompletionSource<RTCSessionDescription>();
            Wake();
            return _capabilitiesFinalTcs.Task.AsAsyncAction();
        }
        //public void ToggleConnectionHealthStats(bool enable) { }
        //public void ToggleETWStats(bool enable) { }
        public void ToggleRTCStats(bool enable) { }
    }

    internal sealed class MediaDevice
    {
        public MediaDevice(string id, string name) { Id = id; Name = name; }

        public string Id { get; set; }
        public string Name { get; set; }

        public IAsyncOperation<List<DtoCaptureCapability>> GetVideoCaptureCapabilities()
        {
            MediaDevice device = this;
            return Task.Run(async () =>
            {
                var settings = new MediaCaptureInitializationSettings()
                {
                    VideoDeviceId = device.Id,
                    MediaCategory = MediaCategory.Communications,
                };
                using (var capture = new MediaCapture())
                {
                    await capture.InitializeAsync(settings);
                    var caps = capture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoRecord);

                    var arr = new List<DtoCaptureCapability>();
                    foreach (var cap in caps)
                    {
                        if (cap.Type != "Video")
                        {
                            continue;
                        }

                        var videoCap = (VideoEncodingProperties)cap;

                        if (videoCap.FrameRate.Denominator == 0 ||
                        videoCap.FrameRate.Numerator == 0 ||
                        videoCap.Width == 0 ||
                        videoCap.Height == 0)
                        {
                            continue;
                        }
                        var captureCap = new DtoCaptureCapability()
                        {
                            Width = videoCap.Width,
                            Height = videoCap.Height,
                            FrameRate = videoCap.FrameRate.Numerator / videoCap.FrameRate.Denominator,
                        };
                        captureCap.FrameRateDescription = $"{captureCap.FrameRate} fps";
                        captureCap.ResolutionDescription = $"{captureCap.Width} x {captureCap.Height}";
                        captureCap.PixelAspectRatio = new DtoMediaRatio()
                        {
                            Numerator = videoCap.PixelAspectRatio.Numerator,
                            Denominator = videoCap.PixelAspectRatio.Denominator,
                        };
                        captureCap.FullDescription = $"{captureCap.ResolutionDescription} {captureCap.FrameRateDescription}";
                        arr.Add(captureCap);
                    }
                    return arr.GroupBy(o => o.FullDescription).Select(o => o.First()).ToList();
                }
            }).AsAsyncOperation();
        }
    }

    internal sealed class CodecInfo
    {
        public CodecInfo(int id, int clockrate, string name) { }

        public int Clockrate { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }


    internal sealed class MediaAudioTrack : IMediaStreamTrack, IDisposable
    {
        private RtcMediaStreamTrack _track;

        public MediaAudioTrack(RtcMediaStreamTrack track) { _track = track; }

        public bool Enabled { get { return _track.Enabled; } set { _track.Enabled = value; } }
        public string Id { get { return _track.Id; } }
        public string Kind { get { return RtcMediaStreamTrack.ToString(_track.Kind); } }

        public void Stop() { _track.Stop(); }
        public RtcMediaStreamTrack Track { get { return _track; } }

        public void Dispose() { _track.Stop(); }
    }

    internal sealed class MediaVideoTrack : IMediaStreamTrack, IDisposable
    {
        private RtcMediaStreamTrack _track;

        public MediaVideoTrack(RtcMediaStreamTrack track) { _track = track; }

        public bool Enabled { get { return _track.Enabled; } set { _track.Enabled = value; } }
        public string Id { get { return _track.Id; } }
        public string Kind { get { return RtcMediaStreamTrack.ToString(_track.Kind); } }

        public RtcMediaStreamTrack Track {get {return _track;} }

        public bool Suspended { get { return _track.Muted; } set { _track.Muted = value; } }

        public void Stop() { _track.Stop(); }

        public void Dispose() { _track.Stop(); }
    }

    internal sealed class MediaStream
    {
        //public bool Active { get; }
        //public string Id { get; }
        private IList<MediaAudioTrack> _audioTracks;
        private IList<MediaVideoTrack> _videoTracks;
        private IList<IMediaStreamTrack> _mediaTracks;

        public MediaStream()
        {
            _mediaTracks = new List<IMediaStreamTrack>();
        }

        public MediaStream(List<MediaAudioTrack> audioTracks, List<MediaVideoTrack> videoTracks)
        {
            _audioTracks = audioTracks;
            _videoTracks = videoTracks;
            _mediaTracks = new List<IMediaStreamTrack>();
            if (null != audioTracks) { foreach (var track in audioTracks) { _mediaTracks.Add(track); } }
            if (null != videoTracks) { foreach (var track in videoTracks) { _mediaTracks.Add(track); } }
        }

        //public void AddTrack(IMediaStreamTrack track) { }
        public IList<MediaAudioTrack> GetAudioTracks() { if (null == _audioTracks) return null; return new List<MediaAudioTrack>(_audioTracks); }
        //public IMediaStreamTrack GetTrackById(string trackId) { return null; }
        public IList<IMediaStreamTrack> GetTracks() { if (null == _mediaTracks) return null; return new List<IMediaStreamTrack>(_mediaTracks); }
        public IList<MediaVideoTrack> GetVideoTracks() { if (null == _videoTracks) return null; return new List<MediaVideoTrack>(_videoTracks); }
        //public void RemoveTrack(IMediaStreamTrack track) { }
        public void Stop() { }
    }

    internal sealed class RTCMediaStreamConstraints
    {
        public RTCMediaStreamConstraints() { }

        public bool audioEnabled { get; set; }
        public bool videoEnabled { get; set; }
    }

    internal interface IMediaStreamTrack
    {
        bool Enabled { get; set; }
        string Id { get; }
        string Kind { get; }

        void Stop();
    }

    internal sealed class RTCConfiguration
    {
        public RTCConfiguration() { }

        //public RTCBundlePolicy? BundlePolicy { get; set; }
        public IList<RTCIceServer> IceServers { get; set; }
        //public RTCIceTransportPolicy? IceTransportPolicy { get; set; }
    }

    //internal enum RTCBundlePolicy
    //{
    //    Balanced = 0,
    //    MaxBundle = 1,
    //    MaxCompat = 2
    //}

    internal sealed class RTCIceServer
    {
        public RTCIceServer() { }

        public string Credential { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
    }

    //internal enum RTCIceTransportPolicy
    //{
    //    None = 0,
    //    Relay = 1,
    //    NoHost = 2,
    //    All = 3
    //}

    //internal enum RTCIceConnectionState
    //{
    //    New = 0,
    //    Checking = 1,
    //    Connected = 2,
    //    Completed = 3,
    //    Failed = 4,
    //    Disconnected = 5,
    //    Closed = 6
    //}

    //internal enum RTCIceGatheringState
    //{
    //    New = 0,
    //    Gathering = 1,
    //    Complete = 2
    //}

    internal enum RTCSdpType
    {
        Offer = 0,
        //Pranswer = 1,
        Answer = 2
    }

    [DataContract]
    internal sealed class RtcJsonDescription
    {
        public RtcJsonDescription() {}

        public RtcJsonDescription(RtcDescription description)
        {
            HasAudio = description.HasAudio;
            HasVideo = description.HasVideo;
            IceRole = RtcIceTypes.ToString(description.IceRole);
            IceParameters = description.IceParameters.ToJsonString();
            AudioSenderCapabilities = description.AudioSenderCapabilities.ToJsonString();
            VideoSenderCapabilities = description.VideoSenderCapabilities.ToJsonString();
            AudioReceiverCapabilities = description.AudioReceiverCapabilities.ToJsonString();
            VideoReceiverCapabilities = description.VideoReceiverCapabilities.ToJsonString();
            DtlsParameters = description.DtlsParameters.ToJsonString();
        }

        [DataMember]
        public bool HasAudio { get; set; }

        [DataMember]
        public bool HasVideo { get; set; }

        [DataMember]
        public string IceRole { get; set; }

        [DataMember]
        public string IceParameters { get; set; }

        [DataMember]
        public string AudioSenderCapabilities { get; set; }

        [DataMember]
        public string VideoSenderCapabilities { get; set; }

        [DataMember]
        public string AudioReceiverCapabilities { get; set; }

        [DataMember]
        public string VideoReceiverCapabilities { get; set; }

        [DataMember]
        public string DtlsParameters { get; set; }
    }

    internal sealed class RtcDescription
    {
        public RtcDescription(
            bool hasAudio,
            bool hasVideo,
            RtcIceRole iceRole,
            RtcIceParameters iceParams,
            RtcRtpCapabilities audioSenderCaps,
            RtcRtpCapabilities videoSenderCaps,
            RtcRtpCapabilities audioReceiverCaps,
            RtcRtpCapabilities videoReceiverCaps,
            RtcDtlsParameters dtlsParameters
            )
        {
            HasAudio = hasAudio;
            HasVideo = hasVideo;
            IceRole = iceRole;
            IceParameters = iceParams;
            AudioSenderCapabilities = audioSenderCaps;
            VideoSenderCapabilities = videoSenderCaps;
            AudioReceiverCapabilities = audioReceiverCaps;
            VideoReceiverCapabilities = videoReceiverCaps;
            DtlsParameters = dtlsParameters;
        }

        public RtcDescription(RtcJsonDescription description)
        {
            HasAudio = description.HasAudio;
            HasVideo = description.HasVideo;
            IceRole = RtcIceTypes.ToRole(description.IceRole);
            IceParameters = RtcIceParameters.FromJsonString(description.IceParameters);
            AudioSenderCapabilities = RtcRtpCapabilities.FromJsonString(description.AudioSenderCapabilities);
            VideoSenderCapabilities = RtcRtpCapabilities.FromJsonString(description.VideoSenderCapabilities);
            AudioReceiverCapabilities = RtcRtpCapabilities.FromJsonString(description.AudioReceiverCapabilities);
            VideoReceiverCapabilities = RtcRtpCapabilities.FromJsonString(description.VideoReceiverCapabilities);
            DtlsParameters = RtcDtlsParameters.FromJsonString(description.DtlsParameters);
        }

        public bool HasAudio { get; set; }
        public bool HasVideo { get; set; }
        public RtcIceRole IceRole { get; set; }
        public RtcIceParameters IceParameters { get; set; }
        public RtcRtpCapabilities AudioSenderCapabilities { get; set; }
        public RtcRtpCapabilities VideoSenderCapabilities { get; set; }
        public RtcRtpCapabilities AudioReceiverCapabilities { get; set; }
        public RtcRtpCapabilities VideoReceiverCapabilities { get; set; }
        public RtcDtlsParameters DtlsParameters { get; set; }
    }

    internal sealed class RTCSessionDescription
    {
        private string _sdp;

        private RtcDescription _description;

        public RTCSessionDescription(
            RtcDescription description
            )
        {
            Description = description;
        }

        public RTCSessionDescription(RTCSdpType type, string caps) {Sdp = caps;}

        public string Sdp {
            get
            {
                return _sdp;
            }
            set
            {
                _sdp = value;
                var jsonDescription = RtcHelper.DeserializeJSon<RtcJsonDescription>(value);
                _description = new RtcDescription(jsonDescription);
            }
        }

        public RtcDescription Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                var jsonDescription = new RtcJsonDescription(_description);
                _sdp = RtcHelper.SerializeJSon<RtcJsonDescription>(jsonDescription);
            }
        }

        //public RTCSdpType? Type { get; set; }
    }

    //internal enum RTCSignalingState
    //{
    //    Stable = 0,
    //    HaveLocalOffer = 1,
    //    HaveRemoteOffer = 2,
    //    HaveLocalPranswer = 3,
    //    HaveRemotePranswer = 4,
    //    Closed = 5
    //}

    //internal sealed class RTCDataChannel
    //{
    //    public uint BufferedAmount { get; }
    //    public ushort Id { get; }
    //    public string Label { get; }
    //    public ushort? MaxPacketLifeTime { get; }
    //    public ushort? MaxRetransmits { get; }
    //    public bool Negotiated { get; }
    //    public bool Ordered { get; }
    //    public string Protocol { get; }
    //    public webrtc_winrt_api.RTCDataChannelState ReadyState { get; }

    //    public event EventDelegate OnClose;
    //    public event EventDelegate OnError;
    //    public event RTCDataChannelMessageEventDelegate OnMessage;
    //    public event EventDelegate OnOpen;

    //    public void Close() { }
    //    public void Send(IDataChannelMessage data) { }
    //}

    //internal sealed class RTCDataChannelInit
    //{
    //    public RTCDataChannelInit() { }

    //    public ushort? Id { get; set; }
    //    public ushort? MaxPacketLifeTime { get; set; }
    //    public ushort? MaxRetransmits { get; set; }
    //    public bool? Negotiated { get; set; }
    //    public bool? Ordered { get; set; }
    //    public string Protocol { get; set; }
    //}

    //internal enum RTCDataChannelState
    //{
    //    Connecting = 0,
    //    Open = 1,
    //    Closing = 2,
    //    Closed = 3
    //}

    //internal interface IDataChannelMessage
    //{
    //    RTCDataChannelMessageType DataType { get; set; }
    //}

    //internal enum RTCDataChannelMessageType
    //{
    //    String = 0,
    //    Binary = 1
    //}

    internal sealed class RTCPeerConnectionIceEvent
    {
        //public RTCPeerConnectionIceEvent() { }

        public RtcIceCandidate Candidate { get; set; }
    }

    internal sealed class MediaStreamEvent
    {
        //public MediaStreamEvent() { }

        public MediaStream Stream { get; set; }
    }

    internal sealed class RTCStatsReport
    {
        //public RTCStatsReport() { }

        public RTCStatsType StatsType { get; set; }
        //public double Timestamp { get; set; }
        public IDictionary<RTCStatsValueName, object> Values { get; set; }
    }

    internal sealed class RTCStatsReportsReadyEvent
    {
        //public RTCStatsReportsReadyEvent() { }

        public IList<RTCStatsReport> rtcStatsReports { get; set; }
    }

    internal enum RTCStatsType
    {
        StatsReportTypeSession = 0,
        StatsReportTypeTransport = 1,
        StatsReportTypeComponent = 2,
        StatsReportTypeCandidatePair = 3,
        StatsReportTypeBwe = 4,
        StatsReportTypeSsrc = 5,
        StatsReportTypeRemoteSsrc = 6,
        StatsReportTypeTrack = 7,
        StatsReportTypeIceLocalCandidate = 8,
        StatsReportTypeIceRemoteCandidate = 9,
        StatsReportTypeCertificate = 10,
        StatsReportTypeDataChannel = 11
    }

    internal enum RTCStatsValueName
    {
        StatsValueNameActiveConnection = 0,
        StatsValueNameAudioInputLevel = 1,
        StatsValueNameAudioOutputLevel = 2,
        StatsValueNameBytesReceived = 3,
        StatsValueNameBytesSent = 4,
        StatsValueNameDataChannelId = 5,
        StatsValueNamePacketsLost = 6,
        StatsValueNamePacketsReceived = 7,
        StatsValueNamePacketsSent = 8,
        StatsValueNameProtocol = 9,
        StatsValueNameReceiving = 10,
        StatsValueNameSelectedCandidatePairId = 11,
        StatsValueNameSsrc = 12,
        StatsValueNameState = 13,
        StatsValueNameTransportId = 14,
        StatsValueNameAccelerateRate = 15,
        StatsValueNameActualEncBitrate = 16,
        StatsValueNameAdaptationChanges = 17,
        StatsValueNameAvailableReceiveBandwidth = 18,
        StatsValueNameAvailableSendBandwidth = 19,
        StatsValueNameAvgEncodeMs = 20,
        StatsValueNameBandwidthLimitedResolution = 21,
        StatsValueNameBucketDelay = 22,
        StatsValueNameCaptureStartNtpTimeMs = 23,
        StatsValueNameCandidateIPAddress = 24,
        StatsValueNameCandidateNetworkType = 25,
        StatsValueNameCandidatePortNumber = 26,
        StatsValueNameCandidatePriority = 27,
        StatsValueNameCandidateTransportType = 28,
        StatsValueNameCandidateType = 29,
        StatsValueNameChannelId = 30,
        StatsValueNameCodecName = 31,
        StatsValueNameComponent = 32,
        StatsValueNameContentName = 33,
        StatsValueNameCpuLimitedResolution = 34,
        StatsValueNameCurrentDelayMs = 35,
        StatsValueNameDecodeMs = 36,
        StatsValueNameDecodingCNG = 37,
        StatsValueNameDecodingCTN = 38,
        StatsValueNameDecodingCTSG = 39,
        StatsValueNameDecodingNormal = 40,
        StatsValueNameDecodingPLC = 41,
        StatsValueNameDecodingPLCCNG = 42,
        StatsValueNameDer = 43,
        StatsValueNameDtlsCipher = 44,
        StatsValueNameEchoCancellationQualityMin = 45,
        StatsValueNameEchoDelayMedian = 46,
        StatsValueNameEchoDelayStdDev = 47,
        StatsValueNameEchoReturnLoss = 48,
        StatsValueNameEchoReturnLossEnhancement = 49,
        StatsValueNameEncodeUsagePercent = 50,
        StatsValueNameExpandRate = 51,
        StatsValueNameFingerprint = 52,
        StatsValueNameFingerprintAlgorithm = 53,
        StatsValueNameFirsReceived = 54,
        StatsValueNameFirsSent = 55,
        StatsValueNameFrameHeightInput = 56,
        StatsValueNameFrameHeightReceived = 57,
        StatsValueNameFrameHeightSent = 58,
        StatsValueNameFrameRateDecoded = 59,
        StatsValueNameFrameRateInput = 60,
        StatsValueNameFrameRateOutput = 61,
        StatsValueNameFrameRateReceived = 62,
        StatsValueNameFrameRateSent = 63,
        StatsValueNameFrameWidthInput = 64,
        StatsValueNameFrameWidthReceived = 65,
        StatsValueNameFrameWidthSent = 66,
        StatsValueNameInitiator = 67,
        StatsValueNameIssuerId = 68,
        StatsValueNameJitterBufferMs = 69,
        StatsValueNameJitterReceived = 70,
        StatsValueNameLabel = 71,
        StatsValueNameLocalAddress = 72,
        StatsValueNameLocalCandidateId = 73,
        StatsValueNameLocalCandidateType = 74,
        StatsValueNameLocalCertificateId = 75,
        StatsValueNameMaxDecodeMs = 76,
        StatsValueNameMinPlayoutDelayMs = 77,
        StatsValueNameNacksReceived = 78,
        StatsValueNameNacksSent = 79,
        StatsValueNamePlisReceived = 80,
        StatsValueNamePlisSent = 81,
        StatsValueNamePreemptiveExpandRate = 82,
        StatsValueNamePreferredJitterBufferMs = 83,
        StatsValueNameRemoteAddress = 84,
        StatsValueNameRemoteCandidateId = 85,
        StatsValueNameRemoteCandidateType = 86,
        StatsValueNameRemoteCertificateId = 87,
        StatsValueNameRenderDelayMs = 88,
        StatsValueNameRetransmitBitrate = 89,
        StatsValueNameRtt = 90,
        StatsValueNameSecondaryDecodedRate = 91,
        StatsValueNameSendPacketsDiscarded = 92,
        StatsValueNameSpeechExpandRate = 93,
        StatsValueNameSrtpCipher = 94,
        StatsValueNameTargetDelayMs = 95,
        StatsValueNameTargetEncBitrate = 96,
        StatsValueNameTrackId = 97,
        StatsValueNameTransmitBitrate = 98,
        StatsValueNameTransportType = 99,
        StatsValueNameTypingNoiseState = 100,
        StatsValueNameViewLimitedResolution = 101,
        StatsValueNameWritable = 102,
        StatsValueNameCurrentEndToEndDelayMs = 103
    }

    internal sealed class RTCPeerConnectionHealthStats
    {
        public RTCPeerConnectionHealthStats() { }

        public string LocalCandidateType { get; set; }
        public long ReceivedBytes { get; set; }
        public long ReceivedKpbs { get; set; }
        public string RemoteCandidateType { get; set; }
        public long RTT { get; set; }
        public long SentBytes { get; set; }
        public long SentKbps { get; set; }
    }
    internal sealed class ResolutionHelper
    {
        //public ResolutionHelper() { }

        public static event ResolutionChangedEventHandler ResolutionChanged;
    }

    internal sealed class Helper
    {
        // see https://philcurnow.wordpress.com/2013/12/29/serializing-and-deserializing-json-in-c/
        public static string SerializeJSon<T>(T t)
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ds = new DataContractJsonSerializer(typeof(T));
            DataContractJsonSerializerSettings s = new DataContractJsonSerializerSettings();
            ds.WriteObject(stream, t);
            string jsonString = Encoding.UTF8.GetString(stream.ToArray());
            stream.Dispose();
            return jsonString;
        }

        public static T DeserializeJSon<T>(string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)ser.ReadObject(stream);
            return obj;
        }

        public static CodecInfo ToDto(RtcCodecCapability codec, int index)
        {
            return new CodecInfo(index, (int)codec.ClockRate, codec.Name);
        }

        public static IList<CodecInfo> GetCodecs(string kind)
        {
            var caps = RtcSender.GetCapabilities(kind);
            var codecs = caps.Codecs;
            var results = new List<CodecInfo>();

            int index = 0;
            foreach (var codec in codecs)
            {
                ++index;
                results.Add(ToDto(codec, index));
            }
            return results;
        }

        public static void CalculateDeltas(
            RtcMediaDeviceKind kind,
            IList<RtcMediaDeviceInfo> previousList,
            IList<RtcMediaDeviceInfo> newFoundList,
            out List<RtcMediaDeviceInfo> addedOrChangedList,
            out List<RtcMediaDeviceInfo> finalReplacementList
            )
        {
            addedOrChangedList = new List<RtcMediaDeviceInfo>();
            finalReplacementList = new List<RtcMediaDeviceInfo>();

            foreach (var info in newFoundList)
            {
                if (info.Kind != kind) continue;

                bool found = false;
                foreach (var innerInfo in previousList)
                {
                    if (innerInfo.Kind != kind) continue;
                    if (IsMatch(info, innerInfo))
                    {
                        found = true;
                        break;
                    }
                }

                finalReplacementList.Add(info);
                if (!found) { addedOrChangedList.Add(info); }
            }
        }

        public static List<RtcMediaDeviceInfo> Filter(
            RtcMediaDeviceKind kind,
            IList<RtcMediaDeviceInfo> infos
            )
        {
            var results = new List<RtcMediaDeviceInfo>();
            foreach (var info in infos)
            {
                if (kind != info.Kind) continue;
                results.Add(info);
            }
            return results;
        }

        public static bool IsMatch(RtcMediaDeviceInfo op1, RtcMediaDeviceInfo op2)
        {
            if (op1.Kind != op2.Kind) return false;
            if (0 != String.Compare(op1.DeviceID, op2.DeviceID)) return false;
            if (0 != String.Compare(op1.GroupID, op2.GroupID)) return false;
            if (0 != String.Compare(op1.Label, op2.Label)) return false;
            return true;
        }

        public static RtcIceGatherOptions ToGatherOptions(RTCConfiguration configuration)
        {
            var options = new RtcIceGatherOptions();
            options.IceServers = new List<RtcIceServer>();
            foreach (var server in configuration.IceServers)
            {
                var gatherServer = new RtcIceServer();
                gatherServer.URLs = new List<String>();
                gatherServer.URLs.Add(server.Url);
                gatherServer.UserName = server.Username;
                options.IceServers.Add(gatherServer);
            }
            return options;
        }


        public static MediaDevice ToMediaDevice(RtcMediaDeviceInfo device)
        {
            return new MediaDevice(device.DeviceID, device.Label);
        }
        public static IList<MediaDevice> ToMediaDevices(IList<RtcMediaDeviceInfo> devices)
        {
            var results = new List<MediaDevice>();
            foreach (var device in devices)
            {
                results.Add(ToMediaDevice(device));
            }
            return results;
        }
        public static RtcConstraints MakeConstraints(
            bool shouldDoThis,
            RtcConstraints existingConstraints,
            RtcMediaDeviceKind kind,
            MediaDevice device
            )
        {
            if (!shouldDoThis) return existingConstraints;
            if (null == device) return existingConstraints;

            if (null == existingConstraints) existingConstraints = new RtcConstraints();
            RtcMediaTrackConstraints trackConstraints = null;

            switch (kind)
            {
                case RtcMediaDeviceKind.AudioInput: trackConstraints = existingConstraints.Audio; break;
                case RtcMediaDeviceKind.AudioOutput: trackConstraints = existingConstraints.Audio; break;
                case RtcMediaDeviceKind.Video: trackConstraints = existingConstraints.Video; break;
            }
            if (null == trackConstraints) trackConstraints = new RtcMediaTrackConstraints();

            if (null == trackConstraints.Advanced) trackConstraints.Advanced = new List<RtcMediaTrackConstraintSet>();

            var constraintSet = new RtcMediaTrackConstraintSet();
            constraintSet.DeviceId = new RtcConstrainString();
            constraintSet.DeviceId.Parameters = new RtcConstrainStringParameters();
            constraintSet.DeviceId.Parameters.Exact = new RtcStringOrStringList();
            constraintSet.DeviceId.Parameters.Exact.Value = device.Id;
            trackConstraints.Advanced.Add(constraintSet);

            switch (kind)
            {
                case RtcMediaDeviceKind.AudioInput: existingConstraints.Audio = trackConstraints; break;
                case RtcMediaDeviceKind.AudioOutput: existingConstraints.Audio = trackConstraints; break;
                case RtcMediaDeviceKind.Video: existingConstraints.Video = trackConstraints; break;
            }
            return existingConstraints;
        }

        public static RtcMediaStreamTrack findTrack(
            IList<RtcMediaStreamTrack> tracks,
            MediaDevice device
            )
        {
            if (null == device) return null;
            foreach (var track in tracks) { if (track.DeviceId != device.Id) return track; }
            return null;
        }

        public static List<MediaAudioTrack> InsertAudioIfValid(
            bool shouldDoThis,
            List<MediaAudioTrack> existingList,
            IList<RtcMediaStreamTrack> tracks,
            MediaDevice device
            )
        {
            if (!shouldDoThis) return existingList;
            if (null == device) return existingList;
            if (null == tracks) return existingList;

            var found = findTrack(tracks, device);
            if (null == found) return existingList;
            if (null == existingList) existingList = new List<MediaAudioTrack>();
            existingList.Add(new MediaAudioTrack(found));
            return existingList;
        }
        public static List<MediaVideoTrack> InsertVideoIfValid(
            bool shouldDoThis,
            List<MediaVideoTrack> existingList,
            IList<RtcMediaStreamTrack> tracks,
            MediaDevice device
            )
        {
            if (!shouldDoThis) return existingList;
            if (null == device) return existingList;
            if (null == tracks) return existingList;

            var found = findTrack(tracks, device);
            if (null == found) return existingList;
            if (null == existingList) existingList = new List<MediaVideoTrack>();
            existingList.Add(new MediaVideoTrack(found));
            return existingList;
        }
    }
}

#endif //USE_ORTC_API

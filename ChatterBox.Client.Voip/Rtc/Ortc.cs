
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
using System.Threading;

namespace ChatterBox.Client.Voip.Rtc
{
    using AutoLock = ChatterBox.Client.Voip.Utils.AutoLock;
    //using LogLevel = webrtc_winrt_api.LogLevel;
    using DtoCaptureCapability = Common.Media.Dto.CaptureCapability;
    using DtoMediaRatio = Common.Media.Dto.MediaRatio;
    using DtoCaptureCapabilities = Common.Media.Dto.CaptureCapabilities;
    using DtoCodecInfo = Common.Media.Dto.CodecInfo;
    using RtcHelper = ChatterBox.Client.Voip.Rtc.Helper;
    using RtcIceCandidate = ortc_winrt_api.RTCIceCandidate;
    using RtcRtpSender = ortc_winrt_api.RTCRtpSender;
    using RtcRtpReceiver = ortc_winrt_api.RTCRtpReceiver;
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
    using RtcRtpParameters = ortc_winrt_api.RTCRtpParameters;
    using RtcRtpCodecParameters = ortc_winrt_api.RTCRtpCodecParameters;
    using RtcDegradationPreference = ortc_winrt_api.RTCDegradationPreference;
    using RtcRtpEncodingParameters = ortc_winrt_api.RTCRtpEncodingParameters;
    using RtcRtpHeaderExtensionParameters = ortc_winrt_api.RTCRtpHeaderExtensionParameters;
    using RtcRtcpParameters = ortc_winrt_api.RTCRtcpParameters;
    using RtcRtcpFeedback = ortc_winrt_api.RTCRtcpFeedback;
    using RtcRtpFecParameters = ortc_winrt_api.RTCRtpFecParameters;
    using RtcPriorityType = ortc_winrt_api.RTCPriorityType;
    using RtcRtpRtxParameters = ortc_winrt_api.RTCRtpRtxParameters;
    using RtcRtpCodecCapability = ortc_winrt_api.RTCRtpCodecCapability;
    using RtcRtpHeaderExtension = ortc_winrt_api.RTCRtpHeaderExtension;
    using RtcSettings = ortc_winrt_api.Settings;

    internal delegate void OnMediaCaptureDeviceFoundDelegate(MediaDevice param);
    internal delegate void RTCPeerConnectionIceEventDelegate(RTCPeerConnectionIceEvent param);
    internal delegate void MediaStreamEventEventDelegate(MediaStreamEvent param0);
    internal delegate void RTCStatsReportsReadyEventDelegate(RTCStatsReportsReadyEvent param0);
    internal delegate void ResolutionChangedEventHandler(string id, uint width, uint height);
    internal delegate void RTCPeerConnectionHealthStatsDelegate(RTCPeerConnectionHealthStats param);
    internal delegate void StepEventHandler();
    internal delegate void FramesPerSecondChangedEventHandler(string id, string fps);

    //using EventDelegate = webrtc_winrt_api.EventDelegate;
    //using RTCDataChannelMessageEventDelegate = webrtc_winrt_api.RTCDataChannelMessageEventDelegate;
    //using RTCDataChannelEventDelegate = webrtc_winrt_api.RTCDataChannelEventDelegate;

    internal sealed class Engine
    {
        private static Engine _singleton = new Engine();

        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

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
            RtcSettings.ApplyDefaults();
            RtcOrtcWithDispatcher.Setup(dispatcher);
        }

        //public static bool IsTracing() { return false; }
        //public static string LogFileName() { return null; }
        //public static StorageFolder LogFolder() { return null; }
        public static IAsyncOperation<bool> RequestAccessForMediaCapture() { return null; }
        //public static bool SaveTrace(string filename) { return false; }
        public static bool SaveTrace(string host, int port)
        {
            using (var @lock = new AutoLock(Singleton._lock))
            {
                Singleton._tracingHost = host;
                Singleton._tracingPort = (UInt16)port;
            }
            return true;
        }

        public static void SetPreferredVideoCaptureFormat(int frame_width, int frame_height, int fps)
        {
#warning TODO IMPLEMENT SetPreferredVideoCaptureFormat
        }

        public static void StartTracing()
        {
            using (var @lock = new AutoLock(Singleton._lock))
            {
                var port = Singleton._tracingPort;
                if (0 == port) port = 59999;

                string host = Singleton._tracingHost;

                RtcLogger.SetLogLevel(RtcLog.Level.Trace);
                RtcLogger.SetLogLevel(RtcLog.Component.ZsLib, RtcLog.Level.Trace);
                RtcLogger.SetLogLevel(RtcLog.Component.Services, RtcLog.Level.Trace);
                RtcLogger.SetLogLevel(RtcLog.Component.ServicesHttp, RtcLog.Level.Trace);
                RtcLogger.SetLogLevel(RtcLog.Component.OrtcLib, RtcLog.Level.Trace);

                if (!String.IsNullOrEmpty(host))
                {
                    RtcLogger.InstallOutgoingTelnetLogger(host + ":" + port.ToString(), true, null);
                }
                else {
                    RtcLogger.UninstallOutgoingTelnetLogger();
                }

                RtcLogger.InstallTelnetLogger(port, 60, true);
            }
        }

        public static void StopTracing()
        {
            using (var @lock = new AutoLock(Singleton._lock))
            {
                RtcLogger.UninstallOutgoingTelnetLogger();
                RtcLogger.UninstallTelnetLogger();
            }
        }

        public static void SynNTPTime(long current_ntp_time) { }
        public static void UpdateCPUUsage(double cpu_usage) { }
        public static void UpdateMemUsage(long mem_usage) { }
    }

    internal sealed class Media
    {
        private static Media _singleton = new Media();

        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

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
            using (var @lock = new AutoLock(media._lock))
            {
                media._media = new RtcMediaDevices();
                media._media.OnDeviceChange += media.Media_OnDeviceChange;
            }
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

                using (var @lock = new AutoLock(_lock))
                {
                    Task.Run(() =>
                    {
                        foreach (var info in audioCaptureEventList)
                        {
                            OnAudioCaptureDeviceFound(new MediaDevice(info.DeviceID, info.Label));
                        }
                        foreach (var info in videoEventList)
                        {
                            OnVideoCaptureDeviceFound(new MediaDevice(info.DeviceID, info.Label));
                        }
                    });

                    _audioCaptureDevices = audioCaptureReplacementList;
                    _audioPlaybackDevices = audioPlaybackReplacementList;
                    _videoDevices = videoReplacementList;
                }
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
            var useTrack = track.Track;
            if (null == useTrack) return null;

            return useTrack.CreateMediaSource();
        }

        //public IMediaSource CreateMediaStreamSource(MediaVideoTrack track, uint framerate, string id) { return null; }
        public IAsyncOperation<bool> EnumerateAudioVideoCaptureDevices() {
            return Task.Run<bool>(async () =>
            {
                var devices = await RtcMediaDevices.EnumerateDevices();

                var audioCaptureList = RtcHelper.Filter(RtcMediaDeviceKind.AudioInput, devices);
                var audioPlaybackList = RtcHelper.Filter(RtcMediaDeviceKind.AudioOutput, devices);
                var videoList = RtcHelper.Filter(RtcMediaDeviceKind.Video, devices);

                using (var @lock = new AutoLock(_lock))
                {
                    _audioCaptureDevices = audioCaptureList;
                    _audioPlaybackDevices = audioPlaybackList;
                    _videoDevices = videoList;
                }

                return devices.Count > 0;
            }).AsAsyncOperation();
        }


        public IList<MediaDevice> GetAudioCaptureDevices() {
            IList<RtcMediaDeviceInfo> result = null;
            using (var @lock = new AutoLock(_lock))
            {
                result = _audioCaptureDevices;
            }
            return RtcHelper.ToMediaDevices(result);
        }
        public IList<MediaDevice> GetAudioPlayoutDevices() {
            IList<RtcMediaDeviceInfo> result = null;
            using (var @lock = new AutoLock(_lock))
            {
                result = _audioPlaybackDevices;
            }
           return RtcHelper.ToMediaDevices(result);
        }

        public IAsyncOperation<MediaStream> GetUserMedia(RTCMediaStreamConstraints mediaStreamConstraints)
        {
            MediaDevice audioCaptureDevice = null;
            MediaDevice videoDevice = null;
            using (var @lock = new AutoLock(_lock))
            {
                audioCaptureDevice = _audioCaptureDevice;
                videoDevice = _videoDevice;
            }

            return Task.Run<MediaStream>(async () =>
            {
                var constraints = RtcHelper.MakeConstraints(mediaStreamConstraints.audioEnabled, null, RtcMediaDeviceKind.AudioInput, audioCaptureDevice);
                constraints = RtcHelper.MakeConstraints(mediaStreamConstraints.videoEnabled, constraints, RtcMediaDeviceKind.Video, videoDevice);

                if (null == constraints) { return new MediaStream(); }

                var tracks = await RtcMediaDevices.GetUserMedia(constraints);

                var audioTracks = RtcHelper.InsertAudioIfValid(mediaStreamConstraints.audioEnabled, null, tracks, audioCaptureDevice);
                var videoTracks = RtcHelper.InsertVideoIfValid(mediaStreamConstraints.videoEnabled, null, tracks, videoDevice);

                return new MediaStream(audioTracks, videoTracks);
            }).AsAsyncOperation();
        }
        public IList<MediaDevice> GetVideoCaptureDevices() {
            IList<RtcMediaDeviceInfo> result = null;
            using (var @lock = new AutoLock(_lock))
            {
                result = _videoDevices;
            }
            return RtcHelper.ToMediaDevices(result);
        }
        public bool SelectAudioDevice(MediaDevice device) {
            using (var @lock = new AutoLock(_lock))
            {
                _audioCaptureDevice = device;
            }
            return true;
        }
        public bool SelectAudioPlayoutDevice(MediaDevice device)
        {
            using (var @lock = new AutoLock(_lock))
            {
                _audioPlaybackDevice = device;
            }
            return true;
        }
        public void SelectVideoDevice(MediaDevice device)
        {
            using (var @lock = new AutoLock(_lock))
            {
                _videoDevice = device;
            }
        }
    }

    internal sealed class RTCPeerConnection
    {
        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

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
        private RtcRtpSender _audioSender;
        private RtcRtpSender _videoSender;
        private RtcRtpReceiver _audioReceiver;
        private RtcRtpReceiver _videoReceiver;
        private MediaDevice _audioPlaybackDevice;

        private bool _installedIceEvents;

        private EventRegistrationTokenTable<RTCPeerConnectionIceEventDelegate> _onIceCandidate = new EventRegistrationTokenTable<RTCPeerConnectionIceEventDelegate>();

        private event StepEventHandler OnStep;

        /// <summary>
        /// Constructor accepts ICE server configuration from application
        /// layer and sets up RtcIceGather, RtcIceTransport and RtcDtlsTransport
        /// </summary>
        /// <remarks>
        /// The RtcDtlsTransport is created after a DTLS web certificate is created
        /// asynchornously. Once the DTLS certificate is created the DTLS transport
        /// can then be constructed with the certificate.
        /// </remarks>
        public RTCPeerConnection(RTCConfiguration configuration)
        {
            var options = RtcHelper.ToGatherOptions(configuration);

            using (var @lock = new AutoLock(_lock))
            {
                _installedIceEvents = false;

                _iceGatherer = new RtcIceGatherer(options);
                _iceTransport = new RtcIceTransport(_iceGatherer);

                // An internal event handler to continue the setup process various media and
                // device selection APIs are called by the application layer.
                OnStep += RTCPeerConnection_OnStep;
            }

            RtcCertificate.GenerateCertificate("").AsTask<RtcCertificate>().ContinueWith((cert) =>
            {
                using (var @lock = new AutoLock(_lock))
                {
                    // Since the DTLS certificate is ready the RtcDtlsTransport can now be constructed.
                    var certs = new List<RtcCertificate>();
                    certs.Add(cert.Result);
                    _dtlsTransport = new RtcDtlsTransport(_iceTransport, certs);
                    if (_closed) _dtlsTransport.Stop();
                }

                // Kick start the continued setup process.
                Wake();
            });
        }

        /// <summary>
        /// An internal helper to kick start the continued setup process of an ORTC connection with a peer.
        /// </summary>
        private void Wake()
        {
            Task.Run(() => { OnStep(); });
        }

        /// <summary>
        /// Setup of an ORTC connection must be done in 3 main steps:
        /// 1. Obtain the capabilities of the ORTC API.
        /// 2. Setup an ORTC RtcRtpReciever to receiver incoming audio and video media.
        /// 3. Setup an ORTC RtcRtpSender to send outgoing audio and video media.
        /// </summary>
        private void RTCPeerConnection_OnStep()
        {
            using (var @lock = new AutoLock(_lock))
            {
                // If the object was closed before the setup event is triggered then abort early.
                if (_closed)
                {
                    Close();
                    return;
                }

                // Can only obtain and setup the senders and receivers if the RtcDtlsTransport is ready.
                if (null == _dtlsTransport) return;

                // Perform the three main steps to setup the capabilities, and incoming and outgoing media.
                StepGetCapabilities();
                StepSetupReceiver();
                StepSetupSender();
            }
        }

        /// <summary>
        /// Obtain the capabilities of the ORTC engine. The application will need to exchange
        /// these capabilities with the remote peer so they can mutually configuring the codecs,
        /// media and RTP parameters necessary to establish an RTC peer session.
        /// </summary>
        private void StepGetCapabilities()
        {
            if (null == _capabilitiesTcs) return;

            // Obtain basic ICE configuration information like the usernameFragment and password.
            var localParams = _iceGatherer.GetLocalParameters();

            // Obtain the complete capabilities of the RtcRtpSenders/RtcRtpReceivers for both
            // audio and video.
            var audioSenderCaps = RtcRtpSender.GetCapabilities("audio");
            var videoSenderCaps = RtcRtpSender.GetCapabilities("video");
            var audioReceiverCaps = RtcRtpReceiver.GetCapabilities("audio");
            var videoReceiverCaps = RtcRtpReceiver.GetCapabilities("video");
            var dtlsParameters = _dtlsTransport.GetLocalParameters();

            // Find out what type of media the application wants to send, i.e. audio, video, or both.
            bool hasAudio = false;
            bool hasVideo = false;

            if (null != _localStream)
            {
                var audioTracks = _localStream.GetAudioTracks();
                if (null != audioTracks) { hasAudio = (audioTracks.Count > 0); }
                var videoTracks = _localStream.GetVideoTracks();
                if (null != videoTracks) { hasVideo = (videoTracks.Count > 0); }
            }

            // If the remote peer's capabilities are already known at this time do not offer media beyond
            // what the remote side desires even if the local application offers the media devices needed
            // for more.
            if (null != _remoteCapabilities)
            {
                hasAudio = _remoteCapabilities.Description.HasAudio;
                hasVideo = _remoteCapabilities.Description.HasVideo;
            }

            // Put all this information into a "blob" that can be JSONified and sent to the remote party.
            var caps = new RTCSessionDescription(new RtcDescription(hasAudio, hasVideo, _iceRole, localParams, audioSenderCaps, videoSenderCaps, audioReceiverCaps, videoReceiverCaps, dtlsParameters));

            // If the remote peer's codec information is known, readjust the codec ordering to match the
            // remote peer's order to ensure both sender and receiver are using the same codec. This is not
            // a requirement of ORTC but typically the same codec is used bidirectionally.
            if (null != _remoteCapabilities) {RtcHelper.PickLocalCodecBasedOnRemote(caps, _remoteCapabilities);}

            // Remember the capabilities for use later when setting up the RtcRtpSenders/RtcRtpReceivers
            _localCapabilities = caps;

            // Return the "blob" information asynchronously to the calling application.
            _capabilitiesTcs.SetResult(_localCapabilities);
            _capabilitiesTcs = null;
        }

        /// <summary>
        /// Setup ORTC to receive incoming audio and/or video media from the remote peer.
        /// </summary>
        /// <remarks>
        /// The application is given the opportunity to override, modify, or tweak and of the RTP
        /// settings before commiting to a particular set of media options. This section uses the
        /// final set of parameters specificied by the application to setup incoming media.
        /// </remarks>
        private void StepSetupReceiver()
        {
            if (null == _capabilitiesFinalTcs) return;

            MediaAudioTrack incomingAudioTrack = null;
            MediaVideoTrack incomingVideoTrack = null;

            // Figure out of the local and remote desire to send/receive audio and video.
            bool hasAudio = _localCapabilitiesFinal.Description.HasAudio;
            bool hasVideo = _localCapabilitiesFinal.Description.HasVideo;
            if (null != _remoteCapabilities)
            {
                hasAudio = hasAudio && _remoteCapabilities.Description.HasAudio;
                hasVideo = hasVideo && _remoteCapabilities.Description.HasVideo;
            }

            // If audio is desired then setup the incoming audio RTP receiver
            if (hasAudio)
            {
                _audioReceiver = new RtcRtpReceiver(_dtlsTransport);

                // Given the local capabilities offered setup a receiver by converting the capabilities
                // offered into settings needed to configure the receiver using the helper routine and
                // label the receiver stream with identifier "a" for audio.
                var @params = RtcHelper.CapabilitiesToParameters("a", _localCapabilities.Description.AudioReceiverCapabilities);
                _audioReceiver.Receive(@params);
                incomingAudioTrack = new MediaAudioTrack(_audioReceiver.Track);
            }

            // If video is desired then setup the incoming audio RTP receiver
            if (hasVideo)
            {
                _videoReceiver = new RtcRtpReceiver(_dtlsTransport);

                // Given the local capabilities offered setup a receiver by converting the capabilities
                // offered into settings needed to configure the receiver using the helper routine and
                // label the receiver stream with identifier "v" for video.
                var @params = RtcHelper.CapabilitiesToParameters("v", _localCapabilities.Description.VideoReceiverCapabilities);
                _videoReceiver.Receive(@params);
                incomingVideoTrack = new MediaVideoTrack(_videoReceiver.Track);
            }

            // ORTC allows for simulcasting so return the list of audio/video streams created to the
            // application. However, in this particular example only one audio and video stream
            // are used since it's a peer to peer scenario.
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

            // Setup a helper media stream containing all the incoming audio and video media "tracks".
            var mediaStream = new MediaStream(incomingAudioTracks, incomingVideoTracks);
            _remoteStream = mediaStream;
            var evt = new MediaStreamEvent();
            evt.Stream = _remoteStream;
            Task.Run(() => { OnAddStream(evt); });

            // Return to the application that the receving media is ready.
            _capabilitiesFinalTcs.SetResult(_localCapabilitiesFinal);
            _capabilitiesFinalTcs = null;
        }

        /// <summary>
        /// Setup ORTC to sender outgoing audio and/or video media from the remote peer.
        /// </summary>
        /// <remarks>
        /// The application is given the opportunity to override, modify, or tweak and of the RTP
        /// settings before commiting to a particular set of media options. This section uses the
        /// final set of parameters specificied by the application to setup outgoing media.
        /// </remarks>
        private void StepSetupSender()
        {
            // Only setup the sender information if all the information is ready and the remote
            // peer's capabilities are known.
            if (null == _remoteCapabilitiesTcs) return;
            if (null == _localStream) return;
            if (null == _localCapabilitiesFinal) return;
            if (null == _remoteCapabilities) return;

            // Start the ICE engine communication with the remote peer.
            _iceTransport.Start(_iceGatherer, _remoteCapabilities.Description.IceParameters, _remoteCapabilities.Description.IceRole);

            // Start the DTLS transport and expect a connection with a DTLS certificate provide by the remote party.
            _dtlsTransport.Start(_remoteCapabilities.Description.DtlsParameters);

            // If the local and remote party are sending audio then setup the audio RtcRtpSender.
            if (_localCapabilitiesFinal.Description.HasAudio)
            {
                if (null == _audioSender)
                {
                    // Figure out if the application has audio media streams to send to the remote party.
                    var tracks = _localStream.GetAudioTracks();
                    MediaAudioTrack mediaTrack = null;
                    RtcMediaStreamTrack track = null;
                    if (null != tracks) { if (tracks.Count > 0) { mediaTrack = tracks.First(); } }
                    if (null != mediaTrack) track = mediaTrack.Track;

                    if (null != track)
                    {
                        // If a track was found then setup the audio RtcRtpSender.
                        _audioSender = new RtcRtpSender(track, _dtlsTransport);
                        var @params = RtcHelper.CapabilitiesToParameters("a", _remoteCapabilities.Description.AudioReceiverCapabilities);
                        RtcHelper.SetupSenderEncodings(@params);
                        _audioSender.Send(@params);
                    }
                }
            }

            // If the local and remote party are sending video then setup the video RtcRtpSender.
            if (_localCapabilitiesFinal.Description.HasVideo)
            {
                if (null == _videoSender)
                {
                    // Figure out if the application has video media streams to send to the remote party.
                    var tracks = _localStream.GetVideoTracks();
                    MediaVideoTrack mediaTrack = null;
                    RtcMediaStreamTrack track = null;
                    if (null != tracks) { if (tracks.Count > 0) { mediaTrack = tracks.First(); } }
                    if (null != mediaTrack) track = mediaTrack.Track;

                    if (null != track)
                    {
                        // If a track was found then setup the video RtcRtpSender.
                        _videoSender = new RtcRtpSender(track, _dtlsTransport);
                        var @params = RtcHelper.CapabilitiesToParameters("v", _remoteCapabilities.Description.VideoReceiverCapabilities);
                        RtcHelper.SetupSenderEncodings(@params);
                        _videoSender.Send(@params);
                    }
                }
            }

            // Return to the application that the sender media is ready.
            _remoteCapabilitiesTcs.SetResult(_remoteCapabilities);
            _remoteCapabilitiesTcs = null;
        }

        public void SelectAudioPlayoutDevice(MediaDevice device)
        {
            RtcMediaStreamTrack track = null;

            using (var @lock = new AutoLock(_lock))
            {
                _audioPlaybackDevice = device;
                if (null == device) return;
                if (null == _audioReceiver) return;

                track = _audioReceiver.Track;
            }

            if (null == track) return;

            var constraints = RtcHelper.MakeConstraints(true, null, RtcMediaDeviceKind.AudioOutput, device);

            track.ApplyConstraints(constraints.Audio).AsTask();
        }

        public void ToggleETWStats(bool enabled)
        {
#warning ToggleETWStats is a horribly named method!
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
                using (var @lock = new AutoLock(_lock))
                {
                    if (!_installedIceEvents)
                    {
                        _iceGatherer.OnICEGathererLocalCandidate += IceGatherer_OnICEGathererLocalCandidate;
                        _iceGatherer.OnICEGathererCandidateComplete += IceGatherer_OnICEGathererCandidateComplete;
                        _iceGatherer.OnICEGathererLocalCandidateGone += IceGatherer_OnICEGathererLocalCandidateGone;
                    }
                    return _onIceCandidate.AddEventHandler(value);
                }
            }
            remove
            {
                using (var @lock = new AutoLock(_lock))
                {
                    _onIceCandidate.RemoveEventHandler(value);
                }
            }
        }

        private void IceGatherer_OnICEGathererLocalCandidate(RtcIceGathererCandidateEvent evt)
        {
            RTCPeerConnectionIceEvent wrapperEvt = new RTCPeerConnectionIceEvent();
            wrapperEvt.Candidate = evt.Candidate;

            RTCPeerConnectionIceEventDelegate invokeList = null;

            using (var @lock = new AutoLock(_lock))
            {
                invokeList = _onIceCandidate.InvocationList;
            }

            invokeList.Invoke(wrapperEvt);
        }

        private void IceGatherer_OnICEGathererCandidateComplete(RtcIceGathererCandidateCompleteEvent evt)
        {
#warning TODO IMPROVE ICE WIRING IceGatherer_OnICEGathererCandidateComplete
            //throw new NotImplementedException();
        }

        private void IceGatherer_OnICEGathererLocalCandidateGone(RtcIceGathererCandidateEvent evt)
        {
            //throw new NotImplementedException();
#warning TODO IMPROVE ICE WIRING IceGatherer_OnICEGathererLocalCandidateGone
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
            using (var @lock = new AutoLock(_lock))
            {
                _localStream = stream;
            }
            Wake();
        }

        public void Close()
        {
            _iceGatherer.Close();
            _iceTransport.Stop();

            using (var @lock = new AutoLock(_lock))
            {
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
                if (null != _remoteStream) { _remoteStream.Stop(); }
                if (null != _audioSender) { _audioSender.Stop(); }
                if (null != _videoSender) { _videoSender.Stop(); }
                if (null != _audioReceiver) { _audioReceiver.Stop(); }
                if (null != _videoReceiver) { _videoReceiver.Stop(); }
            }
        }

        public IAsyncOperation<RTCSessionDescription> CreateAnswer()
        {
            var capabilitiesTcs = new TaskCompletionSource<RTCSessionDescription>();
            using (var @lock = new AutoLock(_lock))
            {
                _iceRole = RtcIceRole.Controlled;
                _capabilitiesTcs = capabilitiesTcs;
            }
            Wake();
            return capabilitiesTcs.Task.AsAsyncOperation();
        }

        //public RTCDataChannel CreateDataChannel(string label, RTCDataChannelInit init) { return null; }
        public IAsyncOperation<RTCSessionDescription> CreateOffer()
        {
            var capabilitiesTcs = new TaskCompletionSource<RTCSessionDescription>();
            using (var @lock = new AutoLock(_lock))
            {
                _iceRole = RtcIceRole.Controlling;
                _capabilitiesTcs = capabilitiesTcs;
            }
            Wake();
            return capabilitiesTcs.Task.AsAsyncOperation();
        }
        //public RTCConfiguration GetConfiguration() { return null; }
        //public IList<MediaStream> GetLocalStreams() { return null; }
        //public IList<MediaStream> GetRemoteStreams() { return null; }
        //public MediaStream GetStreamById(string streamId) { return null; }
        //public void RemoveStream(MediaStream stream) { }
        public IAsyncAction SetLocalDescription(RTCSessionDescription description)
        {
            var capabilitiesFinalTcs = new TaskCompletionSource<RTCSessionDescription>();
            using (var @lock = new AutoLock(_lock))
            {
                _localCapabilitiesFinal = description;
                _capabilitiesFinalTcs = capabilitiesFinalTcs;
            }
            Wake();
            return capabilitiesFinalTcs.Task.AsAsyncAction();
        }
        public IAsyncAction SetRemoteDescription(RTCSessionDescription description)
        {
            var remoteCapabilitiesTcs = new TaskCompletionSource<RTCSessionDescription>();
            using (var @lock = new AutoLock(_lock))
            {
                _remoteCapabilities = description;
                _remoteCapabilitiesTcs = remoteCapabilitiesTcs;
            }
            Wake();
            return remoteCapabilitiesTcs.Task.AsAsyncAction();
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
        public CodecInfo(int id, int clockrate, string name)
        {
            Id = id;
            Clockrate = clockrate;
            Name = name;
        }

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
        private string _blob;

        private RtcDescription _description;

        public RTCSessionDescription(
            RtcDescription description
            )
        {
            Description = description;
        }

        public RTCSessionDescription(RTCSdpType type, string caps) {Sdp = caps;}

        public string Sdp { get { return Blob; } set { Blob = value; } }

        public string Blob
        {
            get
            {
                return _blob;
            }
            set
            {
                _blob = value;
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
                _blob = RtcHelper.SerializeJSon<RtcJsonDescription>(jsonDescription);
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

    internal sealed class FrameCounterHelper
    {
        public FrameCounterHelper() { }

        public static event FramesPerSecondChangedEventHandler FramesPerSecondChanged;
    }

    internal sealed class Helper
    {
        public static void SelectCodecs(
            RTCSessionDescription description,
            DtoCodecInfo inAudioCodec,
            DtoCodecInfo inVideoCodec
            )
        {
            CodecInfo audioCodec = DtoExtensions.FromDto(inAudioCodec); // converted if you need them as one of these objects intead, contains same data
            CodecInfo videoCodec = DtoExtensions.FromDto(inVideoCodec);

            SelectCodec(description.Description.AudioReceiverCapabilities, audioCodec);
            SelectCodec(description.Description.VideoReceiverCapabilities, videoCodec);

            // force update to json blob
            description.Description = description.Description;
        }

        public static bool SelectAudioPlayoutDevice(
            RTCPeerConnection peerConnection,
            Media media,
            MediaDevice device
            )
        {
            bool result = true;
            if (null == device) return false;
            if (null != media) result = media.SelectAudioPlayoutDevice(device);
            if (null != peerConnection) {peerConnection.SelectAudioPlayoutDevice(device);}
            return true;
        }

        public static void PickLocalCodecBasedOnRemote(
            RTCSessionDescription localCapabiliites,
            RTCSessionDescription remoteCapabiliites
            )
        {
            PickLocalCodecBasedOnRemote(localCapabiliites.Description.AudioReceiverCapabilities, remoteCapabiliites.Description.AudioReceiverCapabilities);
            PickLocalCodecBasedOnRemote(localCapabiliites.Description.VideoReceiverCapabilities, remoteCapabiliites.Description.VideoReceiverCapabilities);
            PickLocalCodecBasedOnRemote(localCapabiliites.Description.AudioSenderCapabilities, remoteCapabiliites.Description.AudioSenderCapabilities);
            PickLocalCodecBasedOnRemote(localCapabiliites.Description.VideoSenderCapabilities, remoteCapabiliites.Description.VideoSenderCapabilities);
        }

        private static void SelectCodec(
            RtcRtpCapabilities caps,
            CodecInfo codec
            )
        {
            if (null == caps) return;
            if (null == codec) return;
            if (null == caps.Codecs) return;

            var finalCodecs = new List<RtcRtpCodecCapability>();

            foreach (var capsCodec in caps.Codecs)
            {
                if (codec.Name != capsCodec.Name) continue;
                finalCodecs.Add(capsCodec);
            }

            foreach (var capsCodec in caps.Codecs)
            {
                if (codec.Name == capsCodec.Name) continue;
                finalCodecs.Add(capsCodec);
            }
        }

        public static void PickLocalCodecBasedOnRemote(
            RtcRtpCapabilities localCaps,
            RtcRtpCapabilities remoteCaps
            )
        {
            if (null == localCaps) return;
            if (null == remoteCaps) return;
            if (null == localCaps.Codecs) return;
            if (null == remoteCaps.Codecs) return;

            var newList = new List<RtcRtpCodecCapability>();

            // insert sort local codecs based on remote codec list
            foreach (var remoteCodec in remoteCaps.Codecs)
            {
                foreach (var localCodec in localCaps.Codecs)
                {
                    if (remoteCodec.Name != localCodec.Name) continue;
                    newList.Add(localCodec);
                }
            }

            // insert sort local codecs based on codec not being found in remote codec list
            foreach (var localCodec in localCaps.Codecs)
            {
                bool found = false;
                foreach (var remoteCodec in remoteCaps.Codecs)
                {
                    if (remoteCodec.Name != localCodec.Name) continue;
                    found = true;
                    break;
                }
                if (found) continue;
                newList.Add(localCodec);
            }

            localCaps.Codecs = newList;
        }

        public static RtcRtpParameters CapabilitiesToParameters(
            string muxId,
            RtcRtpCapabilities caps
            )
        {
            var result = new RtcRtpParameters();

            result.Codecs = new List<RtcRtpCodecParameters>();
            foreach (var codec in caps.Codecs) {result.Codecs.Add(CapabilitiesToParameters(codec));}
            result.DegradationPreference = RtcDegradationPreference.MaintainBalanced;
            result.HeaderExtensions = new List<RtcRtpHeaderExtensionParameters>();
            result.Encodings = new List<RtcRtpEncodingParameters>();
            foreach (var ext in caps.HeaderExtensions) { result.HeaderExtensions.Add(CapabilitiesToParameters(ext)); }
            result.MuxId = muxId;
            result.Rtcp = new RtcRtcpParameters();
            result.Rtcp.Mux = true;
            result.Rtcp.ReducedSize = true;

            return result;
        }

        public static RtcRtpCodecParameters CapabilitiesToParameters(
            RtcRtpCodecCapability caps
            )
        {
            var result = new RtcRtpCodecParameters();

            result.ClockRate = caps.ClockRate;
            result.Maxptime = caps.Maxptime;
            result.Name = caps.Name;
            result.NumChannels = caps.NumChannels;
            result.PayloadType = caps.PreferredPayloadType;
            result.RtcpFeedback = caps.RtcpFeedback;
            result.Parameters = caps.Parameters;

            return result;
        }
        public static RtcRtpHeaderExtensionParameters CapabilitiesToParameters(
            RtcRtpHeaderExtension caps
            )
        {
            var result = new RtcRtpHeaderExtensionParameters();

            result.Encrypt = caps.PreferredEncrypt;
            result.Id = caps.PreferredId;
            result.Uri = caps.Uri;

            return result;
        }

        public static void SetupSenderEncodings(RtcRtpParameters @params)
        {
            var encoding = new RtcRtpEncodingParameters();
            encoding.EncodingId = "e1";
            @params.Encodings.Add(encoding);
        }

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

        public static CodecInfo ToDto(RtcRtpCodecCapability codec, int index)
        {
            return new CodecInfo(index, (int)codec.ClockRate, codec.Name);
        }

        public static IList<CodecInfo> GetCodecs(string kind)
        {
            var caps = RtcRtpSender.GetCapabilities(kind);
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

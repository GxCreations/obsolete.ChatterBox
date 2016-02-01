
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

namespace ChatterBox.Client.Voip.Rtc
{
    //using LogLevel = webrtc_winrt_api.LogLevel;
    using RTCIceCandidate = ortc_winrt_api.RTCIceCandidate;
    using RtcSender = ortc_winrt_api.RTCRtpSender;
    using RtcCodecCapability = ortc_winrt_api.RTCRtpCodecCapability;
    using RtcOrtc = ortc_winrt_api.Ortc;
    using RtcOrtcWithDispatcher = ortc_winrt_api.OrtcWithDispatcher;
    using RtcLog = ortc_winrt_api.Log;
    using RtcLogger = ortc_winrt_api.Logger;
    using RtcMediaDevices = ortc_winrt_api.MediaDevices;
    using RtcMediaDeviceInfo = ortc_winrt_api.MediaDeviceInfo;
    using RtcMediaDeviceKind = ortc_winrt_api.MediaDeviceKind;

    using RTCPeerConnectionHealthStatsDelegate = webrtc_winrt_api.RTCPeerConnectionHealthStatsDelegate;
    using System.Threading.Tasks;    //using RTCDataChannelEventDelegate = webrtc_winrt_api.RTCDataChannelEventDelegate;
    //using RTCPeerConnectionIceStateChangeEventDelegate = webrtc_winrt_api.RTCPeerConnectionIceStateChangeEventDelegate;
    //using EventDelegate = webrtc_winrt_api.EventDelegate;
    //using RTCDataChannelMessageEventDelegate = webrtc_winrt_api.RTCDataChannelMessageEventDelegate;

    internal delegate void OnMediaCaptureDeviceFoundDelegate(MediaDevice param);
    internal delegate void RTCPeerConnectionIceEventDelegate(RTCPeerConnectionIceEvent param);
    internal delegate void MediaStreamEventEventDelegate(MediaStreamEvent param0);
    internal delegate void RTCStatsReportsReadyEventDelegate(RTCStatsReportsReadyEvent param0);
    internal delegate void ResolutionChangedEventHandler(string id, uint width, uint height);


    internal sealed class Engine
    {
        private static Engine _singleton;

        private string _tracingHost = null;
        private UInt16 _tracingPort = 0;

        private static Engine Singleton { get { if (null == _singleton) _singleton = new Engine(); return _singleton; } }

        private static CodecInfo ToDto(RtcCodecCapability codec, int index)
        {
            return new CodecInfo(index, (int)codec.ClockRate, codec.Name);
        }

        private static IList<CodecInfo> GetCodecs(string kind)
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

        //public static void DisableLogging() { }
        //public static void EnableLogging(LogLevel level) { }
        public static IList<CodecInfo> GetAudioCodecs() { return GetCodecs("audio"); }

        //public static double GetCPUUsage() { return 0.0; }
        //public static long GetMemUsage() { return 0; }
        public static IList<CodecInfo> GetVideoCodecs() { return GetCodecs("video"); }

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
                calculateDeltas(RtcMediaDeviceKind.AudioInput, _audioCaptureDevices, devices, out audioCaptureEventList, out audioCaptureReplacementList);

                List<RtcMediaDeviceInfo> audioPlaybackEventList;
                List<RtcMediaDeviceInfo> audioPlaybackReplacementList;
                calculateDeltas(RtcMediaDeviceKind.AudioInput, _audioCaptureDevices, devices, out audioPlaybackEventList, out audioPlaybackReplacementList);

                List<RtcMediaDeviceInfo> videoEventList;
                List<RtcMediaDeviceInfo> videoReplacementList;
                calculateDeltas(RtcMediaDeviceKind.Video, _videoDevices, devices, out videoEventList, out videoReplacementList);

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

        private static void calculateDeltas(
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
                if (!found) {addedOrChangedList.Add(info);}
            }
        }

        private static List<RtcMediaDeviceInfo> filter(
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

        private static bool IsMatch(RtcMediaDeviceInfo op1, RtcMediaDeviceInfo op2)
        {
            if (op1.Kind != op2.Kind) return false;
            if (0 != String.Compare(op1.DeviceID, op2.DeviceID)) return false;
            if (0 != String.Compare(op1.GroupID, op2.GroupID)) return false;
            if (0 != String.Compare(op1.Label, op2.Label)) return false;
            return true;
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
#warning TODO IMPLEMENT SetDisplayOrientation
            return null;
        }

        //public IMediaSource CreateMediaStreamSource(MediaVideoTrack track, uint framerate, string id) { return null; }
        public IAsyncOperation<bool> EnumerateAudioVideoCaptureDevices() {
            return Task.Run<bool>(async () =>
            {
                var devices = await RtcMediaDevices.EnumerateDevices();

                var audioCaptureList = filter(RtcMediaDeviceKind.AudioInput, devices);
                var audioPlaybackList = filter(RtcMediaDeviceKind.AudioOutput, devices);
                var videoList = filter(RtcMediaDeviceKind.Video, devices);

                _audioCaptureDevices = audioCaptureList;
                _audioPlaybackDevices = audioPlaybackList;
                _videoDevices = videoList;

                return devices.Count > 0;
            }).AsAsyncOperation();
        }

        private static MediaDevice ToMediaDevice(RtcMediaDeviceInfo device)
        {
            return new MediaDevice(device.DeviceID, device.Label);
        }
        private static IList<MediaDevice> ToMediaDevices(IList<RtcMediaDeviceInfo> devices)
        {
            var results = new List<MediaDevice>();
            foreach (var device in devices)
            {
                results.Add(ToMediaDevice(device));
            }
            return results;
        }

        public IList<MediaDevice> GetAudioCaptureDevices() { return ToMediaDevices(_audioCaptureDevices); }
        public IList<MediaDevice> GetAudioPlayoutDevices() { return ToMediaDevices(_audioPlaybackDevices); }
        public IAsyncOperation<MediaStream> GetUserMedia(RTCMediaStreamConstraints mediaStreamConstraints) { return null; }
        public IList<MediaDevice> GetVideoCaptureDevices() { return ToMediaDevices(_videoDevices); }
        public bool SelectAudioDevice(MediaDevice device) { _audioCaptureDevice = device; return true; }
        public bool SelectAudioPlayoutDevice(MediaDevice device) { _audioCaptureDevice = device; return true; }
        public void SelectVideoDevice(MediaDevice device) { _audioCaptureDevice = device; }
    }


    internal sealed class RTCPeerConnection
    {
        public RTCPeerConnection(RTCConfiguration configuration) { }

        //public RTCIceConnectionState IceConnectionState { get; }
        //public RTCIceGatheringState IceGatheringState { get; }
        //public RTCSessionDescription LocalDescription { get; }
        //public RTCSessionDescription RemoteDescription { get; }
        //public RTCSignalingState SignalingState { get; }

        public event MediaStreamEventEventDelegate OnAddStream;
        public event RTCPeerConnectionHealthStatsDelegate OnConnectionHealthStats;
        //public event RTCDataChannelEventDelegate OnDataChannel;
        public event RTCPeerConnectionIceEventDelegate OnIceCandidate;
        //public event RTCPeerConnectionIceStateChangeEventDelegate OnIceConnectionChange;
        //public event EventDelegate OnNegotiationNeeded;
        //public event MediaStreamEventEventDelegate OnRemoveStream;
        public event RTCStatsReportsReadyEventDelegate OnRTCStatsReportsReady;
        //public event EventDelegate OnSignalingStateChange;

        public IAsyncAction AddIceCandidate(RTCIceCandidate candidate) { return null; }
        public void AddStream(MediaStream stream) { }
        public void Close() { }
        public IAsyncOperation<RTCSessionDescription> CreateAnswer() { return null; }
        //public RTCDataChannel CreateDataChannel(string label, RTCDataChannelInit init) { return null; }
        public IAsyncOperation<RTCSessionDescription> CreateOffer() { return null; }
        //public RTCConfiguration GetConfiguration() { return null; }
        //public IList<MediaStream> GetLocalStreams() { return null; }
        //public IList<MediaStream> GetRemoteStreams() { return null; }
        //public MediaStream GetStreamById(string streamId) { return null; }
        //public void RemoveStream(MediaStream stream) { }
        public IAsyncAction SetLocalDescription(RTCSessionDescription description) { return null; }
        public IAsyncAction SetRemoteDescription(RTCSessionDescription description) { return null; }
        //public void ToggleConnectionHealthStats(bool enable) { }
        //public void ToggleETWStats(bool enable) { }
        public void ToggleRTCStats(bool enable) { }
    }

    internal sealed class MediaDevice
    {
        public MediaDevice(string id, string name) { Id = id;  Name = name; }

        public string Id { get; set; }
        public string Name { get; set; }

        public IAsyncOperation<IList<CaptureCapability>> GetVideoCaptureCapabilities() { return null; }
    }

    internal sealed class CodecInfo
    {
        public CodecInfo(int id, int clockrate, string name) { }

        public int Clockrate { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }


    internal sealed class CaptureCapability
    {
        //public CaptureCapability(uint width, uint height, uint fps, MediaRatio pixelAspect) { }

        public uint FrameRate { get; }
        public string FrameRateDescription { get; }
        public string FullDescription { get; }
        public uint Height { get; }
        public MediaRatio PixelAspectRatio { get; }
        public string ResolutionDescription { get; }
        public uint Width { get; }
    }

    internal sealed class MediaVideoTrack : IDisposable
    {
        public bool Enabled { get; set; }
        //public string Id { get; }
        //public string Kind { get; }
        public bool Suspended { get; set; }

        public void Dispose() { }
        //public void Stop() { }
    }

    internal sealed class MediaStream
    {
        //public bool Active { get; }
        //public string Id { get; }

        //public void AddTrack(IMediaStreamTrack track) { }
        public IList<MediaAudioTrack> GetAudioTracks() { return null; }
        //public IMediaStreamTrack GetTrackById(string trackId) { return null; }
        public IList<IMediaStreamTrack> GetTracks() { return null; }
        public IList<MediaVideoTrack> GetVideoTracks() { return null; }
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

    internal sealed class MediaAudioTrack : IMediaStreamTrack
    {
        public bool Enabled { get; set; }
        public string Id { get; }
        public string Kind { get; }

        public void Stop() { }
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

    //internal sealed class RTCIceCandidate
    //{
    //    public RTCIceCandidate() { }
    //    //public RTCIceCandidate(string candidate, string sdpMid, ushort sdpMLineIndex) { }

    //    public string Candidate { get; set; }
    //    public string SdpMid { get; set; }
    //    public ushort SdpMLineIndex { get; set; }
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

    internal sealed class RTCSessionDescription
    {
        //public RTCSessionDescription() { }
        public RTCSessionDescription(RTCSdpType type, string sdp) { }

        public string Sdp { get; set; }
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

        public RTCIceCandidate Candidate { get; set; }
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

    internal sealed class ResolutionHelper
    {
        //public ResolutionHelper() { }

        public static event ResolutionChangedEventHandler ResolutionChanged;
    }
}

#endif //USE_ORTC_API

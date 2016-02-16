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
using ChatterBox.Client.Common.Settings;
using ChatterBox.Client.Common.Media;
using ChatterBox.Client.Common.Media.Dto;
using ChatterBox.Client.Presentation.Shared.MVVM;
using Windows.ApplicationModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Microsoft.Practices.Unity;
using ChatterBox.Client.Presentation.Shared.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatterBox.Client.Common.Communication.Voip.Dto;
using ChatterBox.Client.Common.Communication.Voip;

namespace ChatterBox.Client.Presentation.Shared.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        public event Action OnRegistrationSettingsChanged;
        public event Action OnQuitApp;
        private string _domain;
        private ApplicationDataContainer _localSettings;
        private string _signalingServerHost;
        private int _signalingServerPort;
        private IMediaSettingsChannel _mediaSettings;
        private IVoipChannel _voipChannel;
        private NtpService _ntpService;
        private CoreDispatcher _dispatcher;
        private bool _appInsightsEnabled;
        private bool _ntpSyncEnabled = false;
        private Boolean _ntpSyncInProgress = false;
        private string _ntpServerIP = "time.windows.com";
        private bool _rtcTraceEnabled;
        private string _rtcTraceServerIP="127.0.0.1";
        private string _rtcTraceServerPort="55000";
        private bool _etwStatsEnabled = false;
        private readonly string[] incompatibleAudioCodecs =
            new string[] { "CN32000", "CN16000", "CN8000", "red8000", "telephone-event8000" };
        private string SelectedFrameRateId = nameof(SelectedFrameRateId) + "Frame";

        public SettingsViewModel(IUnityContainer container,
                                 CoreDispatcher dispatcher)
        {
            _localSettings = ApplicationData.Current.LocalSettings;
            _dispatcher = dispatcher;

            _mediaSettings = container.Resolve<IMediaSettingsChannel>();
            _voipChannel = container.Resolve<IVoipChannel>();
            _ntpService = container.Resolve<NtpService>();

            _ntpService.OnNTPSyncFailed += handleNtpSynFailed;
            _ntpService.OnNTPTimeAvailable += handleNtpTimeSync;

            CloseCommand = new DelegateCommand(OnCloseCommandExecute);
            SaveCommand = new DelegateCommand(OnSaveCommandExecute);
            QuitAppCommand = new DelegateCommand(OnQuitAppCommandExecute);
            DeleteIceServerCommand = new DelegateCommand<IceServerViewModel>(OnDeleteIceServerCommandExecute);
            AddIceServerCommand = new DelegateCommand(OnAddIceServerCommandExecute);            
        }

        #region Navigation

        public async void OnNavigatedTo()
        {            
            await LoadSettings();
        }

        public void OnNavigatedFrom()
        {
            _mediaSettings.ReleaseDevices();
        }

        public DelegateCommand CloseCommand { get; set; }

        public event Action OnClose;

        private void OnCloseCommandExecute()
        {
            OnClose?.Invoke();
        }
        private void OnQuitAppCommandExecute()
        {
            OnQuitApp?.Invoke();
        }

        private void OnSaveCommandExecute()
        {
            bool registrationSettingChanged = false;
            if (SignalingSettings.SignalingServerPort != SignalingServerPort.ToString())
            {
                SignalingSettings.SignalingServerPort = SignalingServerPort.ToString();
                registrationSettingChanged = true;
            }

            if (SignalingSettings.SignalingServerHost != SignalingServerHost)
            {
                SignalingSettings.SignalingServerHost = SignalingServerHost;
                registrationSettingChanged = true;
            }
            if (RegistrationSettings.Domain != Domain)
            {
                RegistrationSettings.Domain = Domain;
                registrationSettingChanged = true;
            }

            if (registrationSettingChanged)
            {
                OnRegistrationSettingsChanged?.Invoke();
            }

#if WIN10
      SignalingSettings.AppInsightsEnabled = AppInsightsEnabled;
#endif

            if (NtpServerIP!=null)
            {
                _localSettings.Values[nameof(NtpServerIP)] = NtpServerIP;
            }

            if (RTCTraceServerIp != null)
            {
                _localSettings.Values[nameof(RTCTraceServerIp)] = RTCTraceServerIp;
            }

            if (RTCTraceServerPort != null)
            {
                _localSettings.Values[nameof(RTCTraceServerPort)] = RTCTraceServerPort;
            }

            if (SelectedCamera != null)
            {
                _mediaSettings.SetVideoDevice(SelectedCamera);
                _localSettings.Values[nameof(SelectedCamera)] = SelectedCamera.Id;
            }

            if (SelectedMicrophone != null)
            {
                _mediaSettings.SetAudioDevice(SelectedMicrophone);
                _localSettings.Values[nameof(SelectedMicrophone)] = SelectedMicrophone.Id;
            }

            if (SelectedVideoCodec != null)
            {
                _mediaSettings.SetVideoCodec(SelectedVideoCodec);
                _localSettings.Values[nameof(SelectedVideoCodec)] = SelectedVideoCodec.Id;
            }

            if (SelectedAudioCodec != null)
            {
                _mediaSettings.SetAudioCodec(SelectedAudioCodec);
                _localSettings.Values[nameof(SelectedAudioCodec)] = SelectedAudioCodec.Id;
            }

            if (SelectedCapFPSItem != null)
            {
                _mediaSettings.SetPreferredVideoCaptureFormat(new VideoCaptureFormat((int)SelectedCapFPSItem.Width,
                                                              (int)SelectedCapFPSItem.Height,
                                                              (int)SelectedCapFPSItem.FrameRate));
                _localSettings.Values[nameof(SelectedCapResItem)] = SelectedCapResItem;
                _localSettings.Values[SelectedFrameRateId] = (SelectedCapFPSItem != null) ? SelectedCapFPSItem.FrameRate : 0;
            }

            
            if (SelectedAudioPlayoutDevice != null)
            {
                _mediaSettings.SetAudioPlayoutDevice(SelectedAudioPlayoutDevice);
                _localSettings.Values[nameof(SelectedAudioPlayoutDevice)] = SelectedAudioPlayoutDevice.Id;
            }

            var newList = new List<IceServer>();
            foreach (var iceServerVm in IceServers)
            {
                if (iceServerVm.Apply())
                {
                    newList.Add(iceServerVm.IceServer);
                }
            }
            IceServerSettings.IceServers = newList;

            OnCloseCommandExecute();
        }

        private async Task LoadSettings()
        {
            _voipChannel.InitializeRTC();

            SignalingServerPort = int.Parse(SignalingSettings.SignalingServerPort);
            SignalingServerHost = SignalingSettings.SignalingServerHost;
            Domain = RegistrationSettings.Domain;
#if WIN10
            AppInsightsEnabled = SignalingSettings.AppInsightsEnabled;
#endif
            if (_localSettings.Values[nameof(NtpServerIP)] != null)
            {
                NtpServerIP = (string)_localSettings.Values[nameof(NtpServerIP)];
            }

            if (_localSettings.Values[nameof(RTCTraceServerIp)] != null)
            {
                RTCTraceServerIp = (string)_localSettings.Values[nameof(RTCTraceServerIp)];
            }

            if (_localSettings.Values[nameof(RTCTraceServerPort)] != null)
            {
                RTCTraceServerPort = (string)_localSettings.Values[nameof(RTCTraceServerPort)];
            }

            Cameras = new ObservableCollection<MediaDevice>(_mediaSettings.GetVideoCaptureDevices().Devices);
            if (_localSettings.Values[nameof(SelectedCamera)] != null)
            {
                var id = (string)_localSettings.Values[nameof(SelectedCamera)];
                var camera = Cameras.SingleOrDefault(c => c.Id.Equals(id));
                if (camera != null)
                {
                    SelectedCamera = camera;                    
                }
            }
            else
            {
                SelectedCamera = Cameras.FirstOrDefault();
            }
            _mediaSettings.SetVideoDevice(SelectedCamera);

            Microphones = new ObservableCollection<MediaDevice>(_mediaSettings.GetAudioCaptureDevices().Devices);
            if (_localSettings.Values[nameof(SelectedMicrophone)] != null)
            {
                var id = (string)_localSettings.Values[nameof(SelectedMicrophone)];
                var mic = Microphones.SingleOrDefault(m => m.Id.Equals(id));
                if (mic != null)
                {
                    SelectedMicrophone = mic;                    
                }
            }
            else
            {
                SelectedMicrophone = Microphones.FirstOrDefault();
            }
            _mediaSettings.SetAudioDevice(SelectedMicrophone);

            AudioCodecs = new ObservableCollection<CodecInfo>();
            var audioCodecList = _mediaSettings.GetAudioCodecs();
            foreach (var audioCodec in audioCodecList.Codecs)
            {
                if (!incompatibleAudioCodecs.Contains(audioCodec.Name + audioCodec.Clockrate))
                {
                    AudioCodecs.Add(audioCodec);
                }
            }
            if (_localSettings.Values[nameof(SelectedAudioCodec)] != null)
            {
                var audioCodecId = (int)_localSettings.Values[nameof(SelectedAudioCodec)];
                var audioCodec = AudioCodecs.SingleOrDefault(a => a.Id.Equals(audioCodecId));
                if (audioCodec != null)
                {
                    SelectedAudioCodec = audioCodec;
                }
            }
            else
            {
                SelectedAudioCodec = AudioCodecs.FirstOrDefault();
            }
            _mediaSettings.SetAudioCodec(SelectedAudioCodec);

            var videoCodecList = _mediaSettings.GetVideoCodecs().Codecs.OrderBy(codec =>
            {
                switch (codec.Name)
                {
                    case "VP8": return 1;
                    case "VP9": return 2;
                    case "H264": return 3;
                    default: return 99;
                }
            });
            VideoCodecs = new ObservableCollection<CodecInfo>(videoCodecList);
            if (_localSettings.Values[nameof(SelectedVideoCodec)] != null)
            {
                var videoCodecId = (int)_localSettings.Values[nameof(SelectedVideoCodec)];
                var videoCodec = VideoCodecs.SingleOrDefault(v => v.Id.Equals(videoCodecId));
                if (videoCodec != null)
                {
                    SelectedVideoCodec = videoCodec;
                }
            }
            else
            {
                SelectedVideoCodec = VideoCodecs.FirstOrDefault();
            }
            _mediaSettings.SetVideoCodec(SelectedVideoCodec);

            AudioPlayoutDevices = new ObservableCollection<MediaDevice>(_mediaSettings.GetAudioPlayoutDevices().Devices);
            string savedAudioPlayoutDeviceId = null;
            if (_localSettings.Values[nameof(SelectedAudioPlayoutDevice)] != null)
            {
                savedAudioPlayoutDeviceId = (string)_localSettings.Values[nameof(SelectedAudioPlayoutDevice)];
                var playoutDevice = AudioPlayoutDevices.SingleOrDefault(a => a.Id.Equals(savedAudioPlayoutDeviceId));
                if (playoutDevice != null)
                {
                    SelectedAudioPlayoutDevice = playoutDevice;
                }
            }
            else
            {
                SelectedAudioPlayoutDevice = AudioPlayoutDevices.FirstOrDefault();
            }
            _mediaSettings.SetAudioPlayoutDevice(SelectedAudioPlayoutDevice);


            IceServers = new ObservableCollection<IceServerViewModel>(
                IceServerSettings.IceServers.Select(ices => new IceServerViewModel(ices)));
        }

        public DelegateCommand<IceServerViewModel> DeleteIceServerCommand { get; }

        private void OnDeleteIceServerCommandExecute(IceServerViewModel iceServerVm)
        {
            IceServers.Remove(iceServerVm);
        }

        public DelegateCommand AddIceServerCommand { get; }

        private void OnAddIceServerCommandExecute()
        {
            IceServers.Insert(0, new IceServerViewModel(new IceServer()));
        }

        #endregion

        #region ChatterBox settings

        public string Domain
        {
            get { return _domain; }
            set { SetProperty(ref _domain, value);}
        }

        public DelegateCommand SaveCommand { get; set; }

        public DelegateCommand QuitAppCommand { get; set; }

        public string SignalingServerHost
        {
            get { return _signalingServerHost; }
            set { SetProperty(ref _signalingServerHost, value);}
          }

        public int SignalingServerPort
        {
            get { return _signalingServerPort; }
            set { SetProperty(ref _signalingServerPort, value); }
        }

        public string NtpServerIP
        {
          get { return _ntpServerIP; }
          set { SetProperty(ref _ntpServerIP, value); }
        }

        public Boolean NtpSyncInProgress
        {
          get { return _ntpSyncInProgress; }
          set
          {
            if (!SetProperty(ref _ntpSyncInProgress, value))
            {
              return;
            }
          }
        }

        public string RTCTraceServerIp
        {
            get { return _rtcTraceServerIP; }
            set { SetProperty(ref _rtcTraceServerIP, value); }
        }

        public string RTCTraceServerPort
        {
            get { return _rtcTraceServerPort; }
            set { SetProperty(ref _rtcTraceServerPort, value); }
        }

        public string ApplicationVersion
        {
            get
            {
                return "ChatterBox " + string.Format("Version: {0}.{1}.{2}.{3}",
                  Package.Current.Id.Version.Major,
                  Package.Current.Id.Version.Minor,
                  Package.Current.Id.Version.Build,
                  Package.Current.Id.Version.Revision);
            }
        }

        public bool AppInsightsEnabled
        {
            get
            {
                return _appInsightsEnabled;
            }
            set
            {
                SetProperty(ref _appInsightsEnabled, value);
            }
        }
        public bool RTCTraceEnabled
        {
            get
            {
                return _rtcTraceEnabled;
            }
            set
            {
                if (!SetProperty(ref _rtcTraceEnabled, value))
                {
                    return;
                }

                if (_rtcTraceEnabled)
                {
                    _mediaSettings.StartTrace();
                }
                else
                {
                    _mediaSettings.StopTrace();
                    _mediaSettings.SaveTrace(new TraceServerConfig()
                    {
                        Ip = _rtcTraceServerIP,
                        Port = Int32.Parse(_rtcTraceServerPort)
                    });
                }
            }
        }

        public bool ETWStatsEnabled
        {
            get
            {
                return _etwStatsEnabled;
            }
            set
            {
                if (!SetProperty(ref _etwStatsEnabled, value))
                {
                    return;
                }

                _mediaSettings.ToggleETWStats(_etwStatsEnabled);
            }
        }


        public bool NtpSyncEnabled
        {
            get
            {
                return _ntpSyncEnabled;
            }
            set
            {
                if (!SetProperty(ref _ntpSyncEnabled, value))
                {
                    return;
                }

                if (_ntpSyncEnabled)
                {
                    //start ntp server sync
                    NtpSyncInProgress = true;
                    _ntpService.GetNetworkTime(NtpServerIP);

                }
                else
                {
                    //donothing
                    NtpSyncInProgress = false;
                    _ntpService.abortSync();

                }
            }
        }
        public bool IsWin10App
        {
            get
            {
#if WIN10
                    return true;
#else
                return false;
#endif
            }
        }

        #endregion

        #region Media settings

        private ObservableCollection<MediaDevice> _cameras;
        public ObservableCollection<MediaDevice> Cameras
        {
            get { return _cameras; }
            set { SetProperty(ref _cameras, value); }
        }

        private ObservableCollection<MediaDevice> _microphones;
        public ObservableCollection<MediaDevice> Microphones
        {
            get { return _microphones; }
            set { SetProperty(ref _microphones, value); }
        }

        private ObservableCollection<MediaDevice> _audioPlayoutDevices;
        public ObservableCollection<MediaDevice> AudioPlayoutDevices
        {
            get
            {
                return _audioPlayoutDevices;
            }
            set
            {
                SetProperty(ref _audioPlayoutDevices, value);
            }
        }

        private MediaDevice _selectedCamera;
        public MediaDevice SelectedCamera
        {
            get { return _selectedCamera; }
            set { SetProperty(ref _selectedCamera, value); SetSelectedCamera(); }
        }

        private MediaDevice _selectedMicrophone;
        public MediaDevice SelectedMicrophone
        {
            get { return _selectedMicrophone; }
            set
            {
                SetProperty(ref _selectedMicrophone, value);
                var localSettings = ApplicationData.Current.LocalSettings;
            }
        }

        private MediaDevice _selectedAudioPlayoutDevice;
        public MediaDevice SelectedAudioPlayoutDevice
        {
            get { return _selectedAudioPlayoutDevice; }
            set
            {
                SetProperty(ref _selectedAudioPlayoutDevice, value);
            }
        }

        private ObservableCollection<string> _allCapRes = new ObservableCollection<string>();
        public ObservableCollection<string> AllCapRes
        /// <summary>
        /// The list of all capture resolutions.
        /// </summary>
        {
            get { return _allCapRes; }
            set { SetProperty(ref _allCapRes, value); }
        }

        private string _selectedCapResItem;
        public string SelectedCapResItem
        {
            get { return _selectedCapResItem; }
            set { SetProperty(ref _selectedCapResItem, value); SetSelectedCapResItem(); }
        }

        private ObservableCollection<CaptureCapability> _allCapFPS;
        public ObservableCollection<CaptureCapability> AllCapFPS
        /// <summary>
        /// The list of all capture frame rates.
        /// </summary>
        {
            get { return _allCapFPS; }
            set { SetProperty(ref _allCapFPS, value); }
        }

        private CaptureCapability _selectedCapFPSItem;
        public CaptureCapability SelectedCapFPSItem
        /// <summary>
        /// The selected capture frame rate.
        /// </summary>
        {
            get { return _selectedCapFPSItem; }
            set { SetProperty(ref _selectedCapFPSItem, value); }
        }

        private ObservableCollection<CodecInfo> _videoCodecs;
        /// <summary>
        /// The list of video codecs.
        /// </summary>
        public ObservableCollection<CodecInfo> VideoCodecs
        {
            get { return _videoCodecs; }
            set { SetProperty(ref _videoCodecs, value); }
        }

        private ObservableCollection<CodecInfo> _audioCodecs;
        /// <summary>
        /// The list of audio codecs.
        /// </summary>
        public ObservableCollection<CodecInfo> AudioCodecs
        {
            get { return _audioCodecs; }
            set { SetProperty(ref _audioCodecs, value); }
        }

        private CodecInfo _selectedVideoCodec;
        public CodecInfo SelectedVideoCodec
        {
            get { return _selectedVideoCodec; }
            set { SetProperty(ref _selectedVideoCodec, value); }
        }

        private CodecInfo _selectedAudioCodec;
        public CodecInfo SelectedAudioCodec
        {
            get { return _selectedAudioCodec; }
            set { SetProperty(ref _selectedAudioCodec, value); }
        }

        private ObservableCollection<IceServerViewModel> _iceServers;
        public ObservableCollection<IceServerViewModel> IceServers
        {
            get { return _iceServers; }
            set { SetProperty(ref _iceServers, value); }
        }

        private IceServerViewModel _selectedIceServer;
        public IceServerViewModel SelectedIceServer
        {
            get { return _selectedIceServer; }
            set
            {
                if (_selectedIceServer != null) _selectedIceServer.IsSelected = false;
                SetProperty(ref _selectedIceServer, value);
                if (_selectedIceServer != null) _selectedIceServer.IsSelected = true;
            }
        }

        #endregion

        #region NTP sync

        private void handleNtpTimeSync( long ntpTime)
        {
            Debug.WriteLine($"New ntp time: {ntpTime}");
            NtpSyncInProgress = false;
            _mediaSettings.SyncWithNTP(ntpTime);

        }

        private void handleNtpSynFailed()
        {

            NtpSyncInProgress = false;

        }
        #endregion

        #region Media settings helpers

        private async void SetSelectedCamera()
        {
            var capRes = new List<string>();
            CaptureCapabilities captureCapabilities = null;

            if (SelectedCamera == null) return;

            try
            {
                captureCapabilities = await _mediaSettings.GetVideoCaptureCapabilitiesAsync(SelectedCamera);
            }
            catch (Exception ex)
            {
                while (ex is AggregateException && ex.InnerException != null)
                    ex = ex.InnerException;
                string errorMsg = "SetSelectedCamera: Failed to GetVideoCaptureCapabilities (Error: " + ex.Message + ")";
                Debug.WriteLine(errorMsg);
                var msgDialog = new MessageDialog(errorMsg);
                await msgDialog.ShowAsync();
                return;
            }
            if (captureCapabilities == null)
            {
                string errorMsg = "SetSelectedCamera: Failed to GetVideoCaptureCapabilities (Result is null)";
                Debug.WriteLine(errorMsg);
                var msgDialog = new MessageDialog(errorMsg);
                await msgDialog.ShowAsync();
                return;
            }

            var uniqueRes = captureCapabilities.Capabilities.GroupBy(test => test.ResolutionDescription)
                .Select(grp => grp.FirstOrDefault()).ToList();
            CaptureCapability defaultResolution = null;
            foreach (var resolution in uniqueRes)
            {
                if (defaultResolution == null)
                {
                    defaultResolution = resolution;
                }
                capRes.Add(resolution.ResolutionDescription);
                if ((resolution.Width == 640) && (resolution.Height == 480))
                {
                    defaultResolution = resolution;
                }
            }
            string selectedCapResItem = string.Empty;

            if (_localSettings.Values[nameof(SelectedCapResItem)] != null)
            {
                selectedCapResItem = (string)_localSettings.Values[nameof(SelectedCapResItem)];
            }

            AllCapRes = new ObservableCollection<string>(capRes);
            if (!string.IsNullOrEmpty(selectedCapResItem) && AllCapRes.Contains(selectedCapResItem))
            {
                SelectedCapResItem = selectedCapResItem;
            }
            else
            {
                SelectedCapResItem = defaultResolution.ResolutionDescription;
            }
        }

        private async Task<CaptureCapabilities> GetVideoCaptureCapabilities(MediaDevice device)
        {
            return await _mediaSettings.GetVideoCaptureCapabilitiesAsync(SelectedCamera);
        }

        void SetSelectedCapResItem()
        {
            if (AllCapFPS == null)
            {
                AllCapFPS = new ObservableCollection<CaptureCapability>();
            }
            else
            {
                AllCapFPS.Clear();
            }
            var opCap = GetVideoCaptureCapabilities(SelectedCamera);
            opCap.ContinueWith(caps =>
            {
                var fpsList = from cap in caps.Result.Capabilities where cap.ResolutionDescription == SelectedCapResItem select cap;
                var t = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        CaptureCapability defaultFPS = null;
                        uint selectedCapFPSFrameRate = 0;
                        if (_localSettings.Values[SelectedFrameRateId] != null)
                        {
                            selectedCapFPSFrameRate = (uint)_localSettings.Values[SelectedFrameRateId];
                        }

                        foreach (var fps in fpsList)
                        {
                            if (selectedCapFPSFrameRate != 0 && fps.FrameRate == selectedCapFPSFrameRate)
                            {
                                defaultFPS = fps;
                            }
                            AllCapFPS.Add(fps);
                            if (defaultFPS == null)
                            {
                                defaultFPS = fps;
                            }
                        }
                        SelectedCapFPSItem = defaultFPS;
                        _mediaSettings.SetPreferredVideoCaptureFormat(new VideoCaptureFormat((int)SelectedCapFPSItem.Width,
                                                                      (int)SelectedCapFPSItem.Height,
                                                                      (int)SelectedCapFPSItem.FrameRate));
                    });
                var uiTask = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                {
                    OnPropertyChanged(nameof(AllCapFPS));
                }));
            });
        }

        #endregion
    }
}
﻿using ChatterBox.Client.Common.Background;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Signaling;
using ChatterBox.Client.Common.Communication.Voip;
using ChatterBox.Client.Common.Notifications;
using ChatterBox.Client.Common.Signaling;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Presentation.Shared.ViewModels;
using ChatterBox.Client.Presentation.Shared.Views;
using ChatterBox.Client.Universal.Background.Tasks;
using ChatterBox.Client.Universal.Services;
using ChatterBox.Common.Communication.Contracts;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Practices.Unity;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ChatterBox.Client.Common.Avatars;

namespace ChatterBox.Client.Universal
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            WindowsAppInitializer.InitializeAsync(
                WindowsCollectors.Metadata |
                WindowsCollectors.PageView |
                WindowsCollectors.Session);
            TelemetryConfiguration.Active.DisableTelemetry =
                !ChatterBox.Client.Common.Settings.SignalingSettings.AppInsightsEnabled;
            UnhandledException += CurrentDomain_UnhandledException;
            InitializeComponent();
            Suspending += OnSuspending;
            Resuming += OnResuming;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (ChatterBox.Client.Common.Settings.SignalingSettings.AppInsightsEnabled)
            {
                ExceptionTelemetry excTelemetry = new ExceptionTelemetry((Exception)e.Exception);
                excTelemetry.SeverityLevel = SeverityLevel.Critical;
                excTelemetry.HandledAt = ExceptionHandledAt.Unhandled;
                excTelemetry.Timestamp = System.DateTimeOffset.UtcNow;
                var telemetry = new TelemetryClient();
                telemetry.TrackException(excTelemetry);

                telemetry.Flush();
            }
        }

        public UnityContainer Container { get; } = new UnityContainer();

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            ToastNotificationLaunchArguments launchArg = null;

            //TODO: Find out why e.Kind is ActivationKind.Launch even when app is lauched by clicking a toast notification
            //if ((e.Kind == ActivationKind.ToastNotification) && (!string.IsNullOrEmpty(e.Arguments)))
            if ((!string.IsNullOrEmpty(e.Arguments)))
            {
                launchArg = ToastNotificationLaunchArguments.FromXmlString(e.Arguments);
            }
            if (e.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                ProcessLaunchArgument(launchArg);
                return;
            }
            if (e.PreviousExecutionState == ApplicationExecutionState.Suspended)
            {
                Resume();
                ProcessLaunchArgument(launchArg);
                return;
            }

            await AvatarLink.ExpandAvatarsToLocal();

            //Register IoC types
            if (!Container.IsRegistered<HubClient>())
            {
                Container.RegisterInstance(CoreApplication.MainView.CoreWindow.Dispatcher);
                Container.RegisterType<TaskHelper>(new ContainerControlledLifetimeManager());
                Container.RegisterType<HubClient>(new ContainerControlledLifetimeManager());
                Container.RegisterInstance<IForegroundUpdateService>(Container.Resolve<HubClient>(), new ContainerControlledLifetimeManager());
                Container.RegisterInstance<ISignalingSocketChannel>(Container.Resolve<HubClient>(), new ContainerControlledLifetimeManager());
                Container.RegisterInstance<IClientChannel>(Container.Resolve<HubClient>(), new ContainerControlledLifetimeManager());
                Container.RegisterInstance<IVoipChannel>(Container.Resolve<HubClient>(), new ContainerControlledLifetimeManager());
                Container.RegisterInstance<IWebRTCSettingsService>(Container.Resolve<HubClient>(), new ContainerControlledLifetimeManager());
                Container.RegisterType<ISocketConnection, SocketConnection>(new ContainerControlledLifetimeManager());
                Container.RegisterType<NtpService>(new ContainerControlledLifetimeManager());
                Container.RegisterType<MainViewModel>(new ContainerControlledLifetimeManager());
                Container.RegisterType<SettingsViewModel>(new ContainerControlledLifetimeManager());
            }

            Container.Resolve<HubClient>().OnDisconnectedFromHub -= App_OnDisconnectedFromHub;
            Container.Resolve<HubClient>().OnDisconnectedFromHub += App_OnDisconnectedFromHub;
            Container.Resolve<SettingsViewModel>().OnQuitApp += QuitApp;

            ConnectHubClient();

            await RegisterForPush(Container.Resolve<TaskHelper>());

            var signalingTask = await RegisterSignalingTask(Container.Resolve<TaskHelper>(), false);
            if (signalingTask == null)
            {
                var message = new MessageDialog("The signaling task is required.");
                await message.ShowAsync();
                return;
            }

            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainView), Container.Resolve<MainViewModel>());
            }

            ProcessLaunchArgument(launchArg);

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void ConnectHubClient()
        {
            if (!Container.Resolve<HubClient>().IsConnected)
            {
                var client = Container.Resolve<HubClient>();

                // Make this call blocking, since we don't want try sendig message until the hub is connected (especially on a resume)
                var connected = Task.Run(client.Connect).Result;
                if (connected)
                {
                    client.SetForegroundProcessId(
                        ChatterBox.Client.WebRTCSwapChainPanel.WebRTCSwapChainPanel.CurrentProcessId);

                    RegisterForDisplayOrientationChange();
                }
                else
                {
                    ShowDialog("Failed to connect to the Hub!");
                }
            }
        }

        private void App_OnDisconnectedFromHub()
        {
            // Nothing to do here for now. The work should be done in OnSuspending and OnResuming
        }

        private async void Resume()
        {
            // Reconnect the Hub client
            ConnectHubClient();

            // Reconnect the Signaling socket
            if (Container.IsRegistered(typeof(ISocketConnection)))
            {
                if (Container.Resolve<HubClient>().IsConnected)
                {
                    var socketConnection = Container.Resolve<ISocketConnection>();
                    if (!socketConnection.IsConnected)
                        await socketConnection.Connect();
                }
            }

            /* If the call was hung-up while we were suspended, we need to update the UI */
            var contactView = Container.Resolve<MainViewModel>().ContactsViewModel;
            if (contactView.SelectedConversation != null)
            {
                if (contactView.SelectedConversation.IsInCallMode)
                {
                    // By calling Initialize, we force to get the voip state from the background
                    contactView.SelectedConversation.Initialize();
                }
            }

            var client = Container.Resolve<HubClient>();
            if (client.IsConnected)
            {
                client.ResumeVoipVideo();
            }

            Window.Current.Activate();
        }

        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            Debug.WriteLine("App.OnSuspending");
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity

            if (ChatterBox.Client.Common.Settings.SignalingSettings.AppInsightsEnabled)
            {

                var telemetry = new TelemetryClient();
                EventTelemetry eventTel = new EventTelemetry("Application Suspending");
                eventTel.Timestamp = System.DateTimeOffset.UtcNow;
                telemetry.TrackEvent(eventTel);
            }
            var client = Container.Resolve<HubClient>();
            // Disconnect the rendering on the UI.
            // We do it here instead of waiting for the background
            // to notify us because we're about to be suspended.
            client.OnUpdateFrameFormat(new FrameFormat
            {
                IsLocal = true,
                Width = 0,
                Height = 0,
                SwapChainHandle = 0
            });
            client.OnUpdateFrameFormat(new FrameFormat
            {
                IsLocal = false,
                Width = 0,
                Height = 0,
                SwapChainHandle = 0
            });
            // Suspend video capture and rendering in the background.
            client.SuspendVoipVideo();

            deferral.Complete();
        }

        private void OnResuming(object sender, object e)
        {
            Debug.WriteLine("App.OnResuming");
            if (ChatterBox.Client.Common.Settings.SignalingSettings.AppInsightsEnabled)
            {
                var telemetry = new TelemetryClient();
                EventTelemetry eventTel = new EventTelemetry("Application Resuming");
                eventTel.Timestamp = System.DateTimeOffset.UtcNow;
                telemetry.TrackEvent(eventTel);
            }
            Resume();
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            LayoutService.Instance.LayoutRoot = args.Window;
            base.OnWindowCreated(args);
        }

        private void RegisterForDisplayOrientationChange()
        {
            var display_info = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
            display_info.OrientationChanged -= this.OnOrientationChanged;
            display_info.OrientationChanged += this.OnOrientationChanged;

            var client = Container.Resolve<HubClient>();
            client.DisplayOrientationChanged(display_info.CurrentOrientation);
        }

        private void OnOrientationChanged(Windows.Graphics.Display.DisplayInformation sender, object args)
        {
            var client = Container.Resolve<HubClient>();
            client.DisplayOrientationChanged(sender.CurrentOrientation);
        }

        private static async System.Threading.Tasks.Task RegisterForPush(TaskHelper helper, bool registerAgain = true)
        {
            try
            {
                PushNotificationHelper.RegisterPushNotificationChannel();

                var pushNotificationTask = await helper.RegisterTask(nameof(PushNotificationTask), typeof(PushNotificationTask).FullName, 
                  new PushNotificationTrigger(), registerAgain).AsTask();
                if (pushNotificationTask == null)
                {
                    Debug.WriteLine("Push notification background task is not started");
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to register for push notification. Error: {e.Message}");
            }
        }

        private void QuitApp()
        {
          UnRegisterAllBackgroundTask();
          Current.Exit();

        }

        void UnRegisterAllBackgroundTask()
        {
          var helper = new TaskHelper();
          var signalingReg = helper.GetTask(nameof(SignalingTask));
          if (signalingReg != null)
              signalingReg.Unregister(true);

          var regOp = helper.GetTask(nameof(PushNotificationTask));
          if (regOp != null)
              regOp.Unregister(true);
        }

        private static async System.Threading.Tasks.Task<IBackgroundTaskRegistration> RegisterSignalingTask(TaskHelper helper, bool registerAgain = true)
        {
            var signalingTask = helper.GetTask(nameof(SignalingTask)) ??
                    await helper.RegisterTask(nameof(SignalingTask), typeof(SignalingTask).FullName,
                            new SocketActivityTrigger(), registerAgain).AsTask();

            return signalingTask;

        }

        public async void ShowDialog(string message)
        {
            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }


        private void ProcessLaunchArgument(ToastNotificationLaunchArguments launchArg)
        {
            if (launchArg != null)
            {
                switch (launchArg.type)
                {
                    case NotificationType.InstantMessage:
                        Container.Resolve<MainViewModel>().ContactsViewModel.SelectConversation(
                            (string)launchArg.arguments[ArgumentType.FromId]);
                        break;
                }
            }
        }
  }
}

﻿using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using ChatterBox.Client.Presentation.Shared.Views;
using Windows.ApplicationModel.Background;
using ChatterBox.Client.Tasks.Signaling.Win8dot1;
using ChatterBox.Client.Signaling;
using Windows.UI.Popups;
using ChatterBox.Client.Presentation.Shared.ViewModels;
using Microsoft.Practices.Unity;
using ChatterBox.Client.Presentation.Shared.Services;
using ChatterBox.Client.Win8dot1.Services;

namespace ChatterBox.Client.Win8dot1
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private SignalingClient _signalingClient;

        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        public UnityContainer Container { get; } = new UnityContainer();

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Container.RegisterInstance(CoreApplication.MainView.CoreWindow.Dispatcher);

            var signalingTaskHelper = new SignalingTaskHelper();
            var signalingTask = await signalingTaskHelper.RegisterTask();

            signalingTask.Completed +=
                (reg, completedArgs) => { Container.Resolve<ISignalingUpdateService>().RaiseUpdate(); };

            var socketService = new SignalingSocketService(signalingTaskHelper.ControlChannelTrigger);
            _signalingClient = new SignalingClient(socketService);

            Container.RegisterInstance(typeof(ISignalingSocketService), socketService)
                     .RegisterInstance(typeof(SignalingSocketService), socketService)
                     .RegisterType<ISignalingUpdateService, SignalingUpdateService>(new ContainerControlledLifetimeManager());

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null || !String.IsNullOrEmpty(args.Arguments))
            {
                // When the navigation stack isn't restored or there are launch arguments
                // indicating an alternate launch (e.g.: via toast or secondary tile), 
                // navigate to the appropriate page, configuring the new page by passing required 
                // information as a navigation parameter
                if (!rootFrame.Navigate(typeof(MainView), Container.Resolve<MainViewModel>()))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
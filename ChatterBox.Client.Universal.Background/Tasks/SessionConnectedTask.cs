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

using ChatterBox.Client.Common.Background;
using ChatterBox.Client.Common.Communication.Signaling.Dto;
using ChatterBox.Client.Common.Settings;
using ChatterBox.Common.Communication.Messages.Registration;
using Windows.ApplicationModel.Background;

namespace ChatterBox.Client.Universal.Background.Tasks
{
    public sealed class SessionConnectedTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var defferal = taskInstance.GetDeferral();

            var taskHelper = new TaskHelper();

            var signalingTask = taskHelper.GetTask(nameof(SignalingTask));

            var connOwner = new ConnectionOwner
            {
                OwnerId = signalingTask.TaskId.ToString()
            };

            Hub.Instance.SignalingSocketService.ConnectToSignalingServer(connOwner);

            Hub.Instance.SignalingClient.Register(new Registration
            {
                Name = RegistrationSettings.Name,
                UserId = RegistrationSettings.UserId,
                Domain = RegistrationSettings.Domain,
                PushNotificationChannelURI = RegistrationSettings.PushNotificationChannelURI
            });

            defferal.Complete();
        }
    }
}

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

using ChatterBox.Client.Common.Communication.Foreground;
using ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Client.Common.Communication.Foreground.Dto.ChatterBox.Client.Common.Communication.Foreground.Dto;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Serialization;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace ChatterBox.Client.Universal.Background
{
    public sealed class ForegroundClient : IForegroundChannel
    {
        #region IForegroundChannel Members

        public void OnSignaledPeerDataUpdated()
        {
            SendToForeground();
        }

        public void OnSignaledRegistrationStatusUpdated()
        {
            SendToForeground();
        }

        public void OnSignaledRelayMessagesUpdated()
        {
            SendToForeground();
        }

        public void OnVoipState(VoipState state)
        {
            SendToForeground(state);
        }

        public void OnUpdateFrameFormat(FrameFormat frameFormat)
        {
            SendToForeground(frameFormat);
        }

        public void OnUpdateFrameRate(FrameRate frameRate)
        {
            SendToForeground(frameRate);
        }

        public ForegroundState GetForegroundState()
        {
            return SendToForeground<ForegroundState>();
        }

        public string GetShownUserId()
        {
            return SendToForeground<string>();
        }

        #endregion

        private ValueSet SendToForeground(object arg = null, [CallerMemberName] string method = null)
        {
            if (Hub.Instance.ForegroundConnection == null)
                return null;

            var channelWriteHelper = new ChannelWriteHelper(typeof(IForegroundChannel));
            var message = channelWriteHelper.FormatOutput(arg, method);
            var sendMessageTask = Hub.Instance.ForegroundConnection.SendMessageAsync(new ValueSet
            {
                {typeof (IForegroundChannel).Name, message}
            }).AsTask();
            sendMessageTask.Wait();
            return sendMessageTask.Result.Status != AppServiceResponseStatus.Success
                ? null
                : sendMessageTask.Result.Message;
        }

        private TResult SendToForeground<TResult>(object arg = null, [CallerMemberName] string method = null)
            where TResult : class
        {
            var resultMessage = SendToForeground(arg, method);
            if (resultMessage == null) return null;
            if (!resultMessage.Values.Any()) return null;
            return (TResult) JsonConvert.Deserialize(resultMessage.Values.Single().ToString(), typeof (TResult));
        }
    }
}
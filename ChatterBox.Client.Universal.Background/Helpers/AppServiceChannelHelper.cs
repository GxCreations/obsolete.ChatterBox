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
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using ChatterBox.Common.Communication.Helpers;
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Client.Universal.Background.Helpers
{
    public static class AppServiceChannelHelper
    {
        /// <summary>
        /// Invokes the method from the message on the handler object and sends back the return value
        /// </summary>
        public static async void HandleRequest(AppServiceRequest request, object handler, string message)
        {
            var invoker = new ChannelInvoker(handler);
            var result = invoker.ProcessRequest(message);
            await SendResponse(request, result);
        }

        /// <summary>
        /// Sends a new message using the AppServiceConnection, containing the contract name, the method name and the serialized argument
        /// </summary>
        public static ValueSet InvokeChannel(this AppServiceConnection connection, Type contractType, object argument,
            string method)
        {

            //Create a new instance of the channel writer and format the message (format: <Method> <Argument - can be null and is serialized as JSON>)
            var channelWriteHelper = new ChannelWriteHelper(contractType);
            var message = channelWriteHelper.FormatOutput(argument, method);

            //Send the message with the key being the contract name and the value being the serialized message
            var sendMessageTask = connection.SendMessageAsync(new ValueSet {{contractType.Name, message}}).AsTask();
            sendMessageTask.Wait();

            //If the message send resulted in a failure, return null, otherwise return the ValueSet result
            return sendMessageTask.Result.Status != AppServiceResponseStatus.Success
                ? null
                : sendMessageTask.Result.Message;
        }

        /// <summary>
        /// Sends a new message using the AppServiceConnection, containing the contract name, the method name and the serialized argument.
        /// The response is deserialized as a object based on the specified response type
        /// </summary>
        public static object InvokeChannel(this AppServiceConnection connection, Type contractType, object argument,
            string method, Type responseType)
        {

            //Invoke the method across the channel, with the specified argument and return the value
            var resultMessage = connection.InvokeChannel(contractType, argument, method);

            //If there is no result, either the invoke failed or the invoked method is of type void
            if (resultMessage == null) return null;
            if (!resultMessage.Values.Any()) return null;

            //Deserialize the result and return the object
            return JsonConvert.Deserialize(resultMessage.Values.Single().ToString(), responseType);
        }


        /// <summary>
        /// Serializes and sends the response for the AppServiceRequest
        /// </summary>
        private static async Task SendResponse(AppServiceRequest request, InvocationResult result)
        {
            if (result.Result == null) return;

            //Send a new ValueSet with the key as a random string and the value as the serialized result
            await request.SendResponseAsync(new ValueSet
            {
                {Guid.NewGuid().ToString(), JsonConvert.Serialize(result.Result)}
            });
        }
    }
}
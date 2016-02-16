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
using System.Reflection;
using ChatterBox.Common.Communication.Serialization;

namespace ChatterBox.Common.Communication.Helpers
{
    public sealed class ChannelInvoker
    {
        /// <summary>
        /// Helper used to invoke a method with an argument on a handler object
        /// </summary>
        /// <param name="handler">The object on which the method will be invoked</param>
        public ChannelInvoker(object handler)
        {
            Handler = handler;
        }

        private object Handler { get; }


        /// <summary>
        /// Handles the request by deserializing it and invoking the requested method on the handler object
        /// </summary>
        /// <param name="request">The serialized request in the format <Method> <Argument - can be null and is serialized as JSON></param>
        public InvocationResult ProcessRequest(string request)
        {
            try
            {
                //Get the method name from the request
                var methodName = !request.Contains(" ")
                    ? request
                    : request.Substring(0, request.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase));

                //Find the method on the handler
                var methods = Handler.GetType().GetRuntimeMethods();
                var method = methods.Single(s => s.Name == methodName);

                //Get the method parameters
                var parameters = method.GetParameters();

                object argument = null;

                //If the method requires parameters, deserialize the parameter based on the required type
                if (parameters.Any())
                {
                    var serializedParameter =
                        request.Substring(request.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase) + 1);
                    argument = JsonConvert.Deserialize(serializedParameter, parameters.Single().ParameterType);
                }

                //Invoke the method on Handler and return the result
                var result = method.Invoke(Handler, argument == null ? null : new[] {argument});
                return new InvocationResult
                {
                    Invoked = true,
                    Result = result
                };
            }
            catch(Exception exception)
            {
                //Return a failed invocation result with the Exception message
                return new InvocationResult
                {
                    Invoked = false,
                    ErrorMessage = exception.ToString()
                };
            }
        }
    }
}
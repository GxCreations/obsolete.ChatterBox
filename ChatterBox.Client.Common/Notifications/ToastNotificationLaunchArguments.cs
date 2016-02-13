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

using System.Collections.Generic;
using ChatterBox.Client.Common.Helpers;
using System.Runtime.Serialization;
using System;

namespace ChatterBox.Client.Common.Notifications
{
    public enum ArgumentType
    {
        FromId
    }

    public enum NotificationType
    {
        InstantMessage
    }

    [DataContract]
    public sealed class ToastNotificationLaunchArguments
    {

        [DataMember]
        public NotificationType type { get; private set; }
        [DataMember]
        public IDictionary<ArgumentType, object> arguments { get; set; }

        public ToastNotificationLaunchArguments()
        {
            //XmlSerialization needs empty constructor
        }

        public ToastNotificationLaunchArguments(NotificationType type)
        {
            this.type = type;
            arguments = new Dictionary<ArgumentType, object>();
        }

        public string ToXmlString()
        {
            return XmlDataContractSerializationHelper.ToXml(this);
        }

        public static ToastNotificationLaunchArguments FromXmlString(string xmlString)
        {
            ToastNotificationLaunchArguments result = null;
            try
            {
                result = XmlDataContractSerializationHelper.FromXml<ToastNotificationLaunchArguments>(xmlString);
            }
            catch(Exception)
            {
                result = null;
            }
            return result;
        }
    }
}

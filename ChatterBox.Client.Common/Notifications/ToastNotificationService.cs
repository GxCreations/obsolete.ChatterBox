﻿//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace ChatterBox.Client.Common.Notifications
{
    public sealed class ToastNotificationService
    {
        public static void ShowInstantMessageNotification(string fromName, string fromUserId, string imageUri, string message)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            // Set Text
            var toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(fromName));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(message));

            // Set image
            var toastImageAttribute = toastXml.GetElementsByTagName("image").Select(s => ((XmlElement) s)).First();
            toastImageAttribute.SetAttribute("src", imageUri);
            toastImageAttribute.SetAttribute("alt", "logo");

            // toast duration
            var toastNode = toastXml.SelectSingleNode("/toast");
            var xmlElement = (XmlElement) toastNode;
            xmlElement?.SetAttribute("duration", "short");

            ToastNotificationLaunchArguments args = new ToastNotificationLaunchArguments(NotificationType.InstantMessage);
            args.arguments.Add(ArgumentType.FromId, fromUserId);

            xmlElement?.SetAttribute("launch", args.ToXmlString());

            ShowNotification(toastXml);
        }

        private static void ShowNotification(XmlDocument toastXml)
        {
            try
            {
                ToastNotificationManager.CreateToastNotifier("ChatterBoxClientAppId")
                     .Show(new ToastNotification(toastXml));
            }
            catch (System.Exception)
            {
                // Most likely an exception occurs here when the win8 client is used together with 
                // a win10 one on same machine. A thrown exception will prevent some UI components
                // to be updated.
            }
        }

        public static void ShowPresenceNotification(string name, string imageUri, bool isOnline)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            // Set Text
            var toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(name));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(isOnline ? "is now Online" : "is now Offline"));

            // Set image
            var toastImageAttribute = toastXml.GetElementsByTagName("image").Select(s => ((XmlElement) s)).First();
            toastImageAttribute.SetAttribute("src", imageUri);
            toastImageAttribute.SetAttribute("alt", "logo");

            // toast duration
            var toastNode = toastXml.SelectSingleNode("/toast");
            var xmlElement = (XmlElement) toastNode;
            xmlElement?.SetAttribute("duration", "short");

            ShowNotification(toastXml);
        }

        public static void ShowToastNotification(string message)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes.First().AppendChild(toastXml.CreateTextNode(message));

            ShowNotification(toastXml);
        }
    }
}
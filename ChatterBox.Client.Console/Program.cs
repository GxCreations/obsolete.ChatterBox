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

using System.Threading.Tasks;
using ChatterBox.Common.Communication.Messages.Registration;

namespace ChatterBox.Client.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            System.Console.Write("Enter your user ID: ");
            var userId = System.Console.ReadLine();
            System.Console.Title = userId;
            var client = new ChatterBoxConsoleClient();
            client.Connect();
            Task.Run(() =>
            {
                client.Register(new Registration
                {
                    Domain = "chatterbox.microsoft.com",
                    Name = userId,
                    UserId = userId,
                    PushNotificationChannelURI = ""
                });
            });

            System.Console.ReadLine();
        }
    }
}
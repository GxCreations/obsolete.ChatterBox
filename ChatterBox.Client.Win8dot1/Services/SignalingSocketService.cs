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

using ChatterBox.Client.Common.Signaling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatterBox.Client.Win8dot1.Services
{
    internal class SignalingSocketService : ISignalingSocketService
    {
        private ISignalingSocketOperation _signalingSocketOperation;

        public SignalingSocketService(ISignalingSocketOperation signalingSokcetOperation)
        {
            _signalingSocketOperation = signalingSokcetOperation;
        }

        public ISignalingSocketOperation SocketOperation
        {
            get
            {
                return _signalingSocketOperation;
            }
        }
    }
}

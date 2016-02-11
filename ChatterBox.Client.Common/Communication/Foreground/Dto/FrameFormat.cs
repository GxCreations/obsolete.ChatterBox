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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatterBox.Client.Common.Communication.Foreground.Dto
{
    public sealed class FrameFormat
    {
        public bool IsLocal { get; set; }
    
        public Int64 SwapChainHandle { get; set; }

        public UInt32 Width { get; set; }

        public UInt32 Height { get; set; }

        public UInt32 ForegroundProcessId { get; set; }
    }
}

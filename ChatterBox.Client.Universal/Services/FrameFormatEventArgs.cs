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

namespace ChatterBox.Client.Universal.Services
{
  public class FrameFormatEventArgs :  EventArgs
  {
    public FrameFormatEventArgs(Int64 swapChainHandle, UInt32 width, UInt32 height)
    {
      SwapChainHandle = swapChainHandle;
      Width = width;
      Height = height;
    }
    public readonly Int64 SwapChainHandle;
    public readonly UInt32 Width;
    public readonly UInt32 Height; 
  }
}

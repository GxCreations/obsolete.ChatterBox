using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatterBox.Client.Common.Communication.Foreground.Dto
{
    public sealed class FrameRate
    {
        public bool IsLocal { get; set; }

        public String FPS { get; set; }
    }
}

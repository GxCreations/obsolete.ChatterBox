using System.Runtime.Serialization;

namespace ChatterBox.Client.Common.Media.Dto
{
    public sealed class MediaDevice
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }
    }

    public sealed class MediaDevices
    {
        [DataMember]
        public MediaDevice[] Devices { get; set; }
    }

    public sealed class CodecInfo
    {
        [DataMember]
        public int Clockrate { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }
    }

    public sealed class CodecInfos
    {
        [DataMember]
        public CodecInfo[] Codecs { get; set; }
    }

    public sealed class MediaRatio
    {
        [DataMember]
        public System.UInt32 Denominator { get; set; }

        [DataMember]
        public System.UInt32 Numerator { get; set; }
    }

    public sealed class CaptureCapability
    {
        [DataMember]
        public uint FrameRate { get; set; }

        [DataMember]
        public string FrameRateDescription { get; set; }

        [DataMember]
        public string FullDescription { get; set; }

        [DataMember]
        public uint Height { get; set; }

        [DataMember]
        public MediaRatio PixelAspectRatio { get; set; }

        [DataMember]
        public string ResolutionDescription { get; set; }

        [DataMember]
        public uint Width { get; set; }
    }

    public sealed class CaptureCapabilities
    {
        [DataMember]
        public CaptureCapability[] Capabilities { get; set; }
    }

    public sealed class VideoCaptureFormat
    {
        public VideoCaptureFormat()
        {
            Width = Height = FrameRate = 0;
        }

        public VideoCaptureFormat(int width, int height, int frameRate)
        {
            Width = width;
            Height = height;
            FrameRate = frameRate;
        }

        [DataMember]
        public int Width { get; set; }

        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public int FrameRate { get; set; }
    }
}

using System.Xml.Serialization;

namespace Converter.Metadata.StreamInfo
{
    [XmlRoot(Namespace = "", ElementName = "stream")]
    public abstract class StreamInfoBase
    {
        [XmlAttribute(AttributeName = "codec_name")]
        public string CodecName { get; set; }

        [XmlAttribute(AttributeName = "codec_long_name")]
        public string CodecLongName { get; set; }

        [XmlAttribute(AttributeName = "codec_type")]
        public string CodecType { get; set; }

        [XmlAttribute(AttributeName = "codec_time_base")]
        public string CodecTimeBase { get; set; }

        [XmlAttribute(AttributeName = "codec_tag_string")]
        public string CodecTagString { get; set; }

        [XmlAttribute(AttributeName = "codec_tag")]
        public string CodecTag { get; set; }

        [XmlAttribute(AttributeName = "r_frame_rate")]
        public string RealBaseFrameRate { get; set; }

        [XmlAttribute(AttributeName = "avg_frame_rate")]
        public string AvgFrameRate { get; set; }

        [XmlAttribute(AttributeName = "time_base")]
        public string TimeBase { get; set; }

        [XmlAttribute(AttributeName = "start_time")]
        public double StartTime { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public double Duration { get; set; }

        [XmlAttribute(AttributeName = "bit_rate")]
        public int Bitrate { get; set; }

        [XmlAttribute(AttributeName = "nb_frames")]
        public int NumberOfFrames { get; set; }
    }
}

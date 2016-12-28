using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Converter.Metadata.StreamInfo
{
    //TODO добавить Namespace
    [XmlRoot(Namespace = "", ElementName = "stream")]
    public class VideoStreamInfo : StreamInfoBase
    {
        [XmlAttribute(AttributeName = "profile")]
        public string Profile { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public int Width { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public int Height { get; set; }

        [XmlAttribute(AttributeName = "sample_aspect_ratio")]
        public string SampleAspectRatio { get; set; }

        [XmlAttribute(AttributeName = "display_aspect_ratio")]
        public string DisplayAspectRatio { get; set; }

        [XmlAttribute(AttributeName = "pix_fmt")]
        public string PixelFormat { get; set; }

        [XmlAttribute(AttributeName = "level")]
        public int Level { get; set; }

        [XmlAttribute(AttributeName = "is_avc")]
        public bool IsAvc { get; set; }

        [XmlAttribute(AttributeName = "nal_length_size")]
        public string NalLengthSize { get; set; }
    }
}


namespace Converter.Metadata
{
    /// <summary>
    /// Параметры конвертирования видео
    /// </summary>
    public class VideoConvertSettings
    {
        /// <summary>
        /// Высота видео
        /// </summary>
        public int FrameHeight { get; set; }
        /// <summary>
        /// Кадров в секунду
        /// </summary>
        public int FrameRate { get; set; }
        /// <summary>
        /// Формат
        /// </summary>
        public VideoFormat Format { get; set; }


        public VideoConvertSettings(VideoQuality quality, int frameRate, VideoFormat format)
        {
            FrameRate = frameRate;
            Format = format;

            switch (quality)
            {
                case VideoQuality.Vq240P:
                    FrameHeight = 240;
                    break;
                case VideoQuality.Vq360P:
                    FrameHeight = 360;
                    break;
                case VideoQuality.Vq480P:
                    FrameHeight = 480;
                    break;
                case VideoQuality.Vq720P:
                    FrameHeight = 720;
                    break;
            }
        }
    }
}

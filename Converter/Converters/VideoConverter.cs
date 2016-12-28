using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Converter.Metadata;
using Converter.Metadata.StreamInfo;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Converter.Converters
{
	/// <summary>
	/// Конвертер файлов
	/// </summary>
    public class VideoConverter
    {
        /// <summary>
        /// Путь к директории с бинарниками ffmpeg
        /// </summary>
        public string FfmpegPath { get; set; }
            
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="ffmpegPath">Путь к директории bin</param>
        public VideoConverter(string ffmpegPath)
        {
            if (!Directory.Exists(ffmpegPath))
            {
                throw new DirectoryNotFoundException(Resource.DirectoryNotFoundMessage);
            }

            FfmpegPath = ffmpegPath;
        }

        /// <summary>
        /// Конвертирует видео-файл
        /// </summary>
        /// <param name="outputPath">Путь к сконвертированному файлу</param>
        /// <param name="video">Параметры конвертации</param>
        public void ConvertVideo(string source, string outputPath, VideoConvertSettings video)
        {
            var videoStream = GetVideoStream(source);

            if (videoStream == null)
            {
                throw new Exception(Resource.VideoStreamNotFound);
            }

            string codecs;

            //на будущие форматы
            switch (video.Format)
            {
                case VideoFormat.mp4:
                    codecs = "-f mp4";
                    break;                
                default:
                    codecs = "-f mp4";
                    break;
            }

            if (video.FrameHeight > videoStream.Height)
            {
                video.FrameHeight = videoStream.Height;
            }

			int width = videoStream.Width * video.FrameHeight / videoStream.Height;

			if (width % 2 != 0)
			{
				width++;
			}

			string arguments = string.Format("-i \"{0}\" -y -r {4} -vf \"scale={5}:{1}\" {2} \"{3}\"",
                                                source, video.FrameHeight, codecs, outputPath, video.FrameRate, width);

            var output = Run("ffmpeg", arguments);
            if (!File.Exists(outputPath))
            {
                throw new Exception(output);
            }
        }

        /// <summary>
        /// Получает метаданные о видеопотоках файла
        /// </summary>
        /// <returns></returns>
        /// <exception cref="T:System.IO.FileNotFoundException">Указанный файл не найден</exception>
        public VideoStreamInfo GetVideoStream(string source)
        {
            if (!File.Exists(source))
            {
                throw new FileNotFoundException("Файл не найден", source);
            }

            VideoStreamInfo result = new VideoStreamInfo();

            // запускаем ffprobe с выдачей результата в формате xml
            string xmlData = Run("ffprobe", string.Format("-v quiet -print_format xml -show_streams {0}", source));
            XElement xData;

            try
            {
                xData = XElement.Parse(xmlData);
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка получения метаданных файла", e);
            }

            //TODO дорботть поиск видеопотока
            // получаем данные о потоках файла
            var xStreams = xData.Elements("streams")                ;

            if (xStreams == null || xStreams.Count() == 0)
            {
                throw new Exception("Файл не имеет видео потоков");
            }

            foreach (var xStream in xStreams.Elements())
            {
                XAttribute xCodecType = xStream.Attribute("codec_type");

                if (xCodecType == null)
                {
                    continue;
                }

                if (xCodecType.Value == "video")
                {
                    return DeserializeStreamInfo<VideoStreamInfo>(xStream.ToString());
                }               
            }

            return result;
        }

        private string Run(string exe, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.Combine(FfmpegPath, exe);
            psi.Arguments = arguments;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            using (Process exeProcess = new Process())
            {
                exeProcess.StartInfo = psi;
                exeProcess.Start();

                StreamReader output = exeProcess.StandardOutput;
                StreamReader error = exeProcess.StandardError;

                StringBuilder outputBuilder = new StringBuilder();
                do
                {
                    ProcessStreamData(error, outputBuilder);
                    ProcessStreamData(output, outputBuilder);
                }
                while (!exeProcess.HasExited);

                ProcessStreamData(error, outputBuilder);
                ProcessStreamData(output, outputBuilder);

                var res = outputBuilder.ToString();

                return res;
            }
        }

        private void ProcessStreamData(StreamReader reader, StringBuilder outputBuffer)
        {
            string outputPart = reader.ReadToEnd();

            if (!string.IsNullOrWhiteSpace(outputPart))
            {
                outputBuffer.Append(outputPart);
            }
        }

        private T DeserializeStreamInfo<T>(string xml) where T : StreamInfoBase
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            StringReader xmlStringReader = new StringReader(xml);
            return ser.Deserialize(xmlStringReader) as T;
        }
    }
}

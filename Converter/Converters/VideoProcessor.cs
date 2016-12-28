using System;
using System.IO;
using Converter.Metadata;

namespace Converter.Converters
{
	/// <summary>
	/// Обработчик задач на конвертацию и сохранение файлов
	/// </summary>
    public class VideoProcessor
    {
        /// <summary>
        /// Временная папка
        /// </summary>
        private string _tempDirectory { get; }
        /// <summary>
        /// Конвертер видеофайлов
        /// </summary>
        private VideoConverter _сonverter { get; }
        
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="ffmpegPath">Путь к директории с исполняемыми файлами ffmpeg</param>
        /// <param name="tempDirectory">Временная папка для обработки файлов</param>
        public VideoProcessor(string ffmpegPath, string tempDirectory)
        {
            _tempDirectory = string.IsNullOrEmpty(tempDirectory) ? Path.GetTempPath() : tempDirectory;
            _сonverter = new VideoConverter(ffmpegPath);
        }

        /// <summary>
        /// Обработка видеофайла
        /// </summary>
        /// <param name="source">Исходный файл</param>
        /// <param name="destination">Путь назначения</param>
        /// <param name="quality">Качество видео (используем пока только 240p) </param>
        /// <param name="videoFrameRate">Частота кадров видео (используем пока только 24)</param>
        /// <param name="format">Формат видео (используем пока только mp4)</param>
        public void ProcessVideo(string source, 
            string destination, 
            VideoQuality quality, 
            int videoFrameRate, 
            VideoFormat format)
        {
            string extension = string.Format(".{0}", Enum.GetName(typeof(VideoFormat), format));
			string destinationFileName = Path.GetFileNameWithoutExtension(source) + extension;
            string outputTempFileName = Path.Combine(_tempDirectory, Guid.NewGuid().ToString());
			
            try
            {
	            if (!Directory.Exists(destination))
	            {
		            Directory.CreateDirectory(destination);
	            }

	            destinationFileName = Path.Combine(destination, destinationFileName);

                VideoConvertSettings convSettings = new VideoConvertSettings(quality, videoFrameRate, format);
                // конвертируем и сохраняем во временном файле
                _сonverter.ConvertVideo(source, outputTempFileName, convSettings);

                // upload to storage
                using (FileStream fileStream = File.OpenRead(outputTempFileName))
                {
                    using (FileStream writeStream = System.IO.File.Create(destinationFileName))
                    {
                        fileStream.CopyTo(writeStream);
                    }                    
                }

            }
            finally
            {
              if (File.Exists(outputTempFileName))
                {
                    File.Delete(outputTempFileName);
                }
            }
        }
    }
}

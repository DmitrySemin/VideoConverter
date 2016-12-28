using NLog;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Common.Messages;
using Common.Models.Settings;

namespace Common.Utilities
{
	/// <summary>
	/// Обработчик файлов в исходном хранилище
	/// </summary>
	public static class FilesHelper
    {
		/// <summary>
		/// Логгер
		/// </summary>
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		/// <summary>
		/// Доступные для декодирования форматы файлов
		/// </summary>
        private static string[] _mediaExtensions = { ".WMA", ".OGG", ".AVI", ".MP4", ".DIVX", ".WMV"};

        /// <summary>
        /// Формирование списка файлов для обработки
        /// </summary>
        /// <param name="settings">Настройка кластера</param>
        /// <returns>Очередь с информацией о файлах</returns>
        public static ConcurrentQueue<ProcessFileInfo> GetFilesToConvert(ClusterSettings settings)
        {
            DirectoryInfo directory = new DirectoryInfo(settings.InputPath);

            try
            {
                return new ConcurrentQueue<ProcessFileInfo>(
                    directory.EnumerateFiles("*", SearchOption.AllDirectories)
                    .Where(f => IsVideoFile(f))
                    .Select(fi => new ProcessFileInfo()
                    {
                        TaskId = Guid.NewGuid(),
                        FilePath = fi.FullName,
                        Length = fi.Length, 
                        DestinationPath = GetDestinationPath(settings, fi)
                    }));
            }
            catch (DirectoryNotFoundException)
            {
                _logger.Error(Resource.ErrorDirectoryNotFound);
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                _logger.Error(Resource.ErrorUnauthorizedAccess);
                throw;
            }
            catch (PathTooLongException)
            {
                _logger.Error(Resource.ErrorPathTooLong);
                throw;
            }
        }
        
		/// <summary>
		/// Проверка на соответствие формата
		/// </summary>
        private static bool IsVideoFile(FileInfo file)
        {
            return -1 != Array.IndexOf(_mediaExtensions, Path.GetExtension(file.Name).ToUpperInvariant());
        }

		/// <summary>
		/// Формирование пути для сохранения файла
		/// </summary>
        private static string GetDestinationPath(ClusterSettings settings, FileInfo fileInfo)
        {
            return Path.Combine(settings.OutputPath,
                fileInfo.DirectoryName.Replace(Path.GetPathRoot(fileInfo.DirectoryName), ""));
        }
    }
}

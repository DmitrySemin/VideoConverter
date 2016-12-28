using System;
using Common.Models;
using Common.Models.Settings;

namespace Common.Messages
{
	/// <summary>
	/// Контейнер информации об обработке файла
	/// </summary>
    public class ProcessFileInfo
    {
		/// <summary>
		/// ID задания в очереди
		/// </summary>
        public Guid TaskId { get; set; }
		/// <summary>
		/// Путь к исходному фалу
		/// </summary>
        public string FilePath { get; set; }
		/// <summary>
		/// Длина файла (для последующих расчетов прогресса)
		/// </summary>
        public long Length { get; set; }
		/// <summary>
		/// Папка назначения
		/// </summary>
        public string DestinationPath { get; set; }
		/// <summary>
		/// Статус обработки
		/// </summary>
        public ProcessStatus ProcessStatus { get; set; }
		/// <summary>
		/// Начало обработки (время постановки в очередь)
		/// </summary>
        public DateTime? StartDate { get; set; }
		/// <summary>
		/// Адрес ноды
		/// </summary>
        public string Node { get; set; }
    }
}

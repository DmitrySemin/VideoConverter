using System;

namespace Common.Messages
{
	/// <summary>
	/// Сообщение о завершении таска обработки
	/// </summary>
	public class TaskComplete
    {
		/// <summary>
		/// Адрес ноды
		/// </summary>
		public string Host { get; set; }
		/// <summary>
		/// ID задания в очереди
		/// </summary>
		public Guid TaskId { get; set; }
    }
}

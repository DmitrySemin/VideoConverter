using System;
using System.Collections.Generic;

namespace Common.Models
{
	/// <summary>
	/// Информация о подключенных нодах
	/// </summary>
    public class NodeInfo
    {
		/// <summary>
		/// Наименование ноды
		/// </summary>
        public string Name { get; set; }
		/// <summary>
		/// Статус (ограничился простой строкой)
		/// </summary>
        public string Status { get; set; }
		/// <summary>
		/// Дата последнего сообщения хартбита
		/// </summary>
		public DateTime LastReadyMessage { get; set; }
		/// <summary>
		/// Количество акторов
		/// </summary>
		public int? ActorsCount { get; set; }
		/// <summary>
		/// Ошибки
		/// </summary>
        public IList<string> Errors { get; set; }
    }
}

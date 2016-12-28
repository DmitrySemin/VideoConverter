using System.Collections.Generic;

namespace Common.Models.Responses
{
	/// <summary>
	/// Модель информации о происходящем для фронта
	/// </summary>
	public class ProgressInfoResponse
	{
		/// <summary>
		/// Общий объем данных в мегабайтах
		/// </summary>
		public long All { get; set; }
		/// <summary>
		/// Оставшийся объем в мегабайтах
		/// </summary>
		public long Remain { get; set; }
		/// <summary>
		/// Скороcть обработки в секундах
		/// </summary>
		public long Speed { get; set; }
		/// <summary>
		/// Оставшееся время в минутах
		/// </summary>
		public long RemainTime { get; set; }
		/// <summary>
		/// Информация о нодах
		/// </summary>
		public List<NodeInfo> Nodes { get; set; }
	}
}
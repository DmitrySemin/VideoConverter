namespace Common.Messages
{
	/// <summary>
	/// Сообщение о старте ноды
	/// </summary>
    public class NodeStarted
    {
		/// <summary>
		/// Адрес ноды
		/// </summary>
		public string Host { get; set; }
		/// <summary>
		/// Количество акторов в ноде
		/// </summary>
		public int ActorsCount { get; set; }
    }
}

namespace Common.Messages
{
	/// <summary>
	/// Сообщение о существовании ноды (heartbeat)
	/// </summary>
	public class NodeReady
    {
		/// <summary>
		/// Адрес ноды
		/// </summary>
		public string Host { get; set; }
    }
}

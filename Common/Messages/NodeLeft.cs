namespace Common.Messages
{
	/// <summary>
	/// Сообщение об отключении ноды
	/// </summary>
    public class NodeLeft
	{      
		/// <summary>
		/// Адрес ноды
		/// </summary>
		public string Host { get; set; }
    }
}

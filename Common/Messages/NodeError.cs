namespace Common.Messages
{
	/// <summary>
	/// Сообщение об ошибке
	/// </summary>
    public class NodeError
    {
		/// <summary>
		/// Адрес ноды
		/// </summary>
        public string Host { get; set; }
		/// <summary>
		/// Ошибка
		/// </summary>
        public string Error { get; set; }
    }
}

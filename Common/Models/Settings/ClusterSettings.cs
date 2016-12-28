namespace Common.Models.Settings
{
	/// <summary>
	/// Настройки кластера
	/// </summary>
    public class ClusterSettings
    {
        /// <summary>
        /// Исходный каталог
        /// </summary>
        public string InputPath { get; set; }
        /// <summary>
        /// Каталог назначения
        /// </summary>
        public string OutputPath { get; set; }
    }
}

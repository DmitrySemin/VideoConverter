namespace Common.Models.Settings
{
    /// <summary>
    /// Настройки ноды
    /// </summary>
    public class NodeSettings
    {
        /// <summary>
        /// Путь к бинарникам ffmpeg
        /// </summary>
        public string FfMpegPath { get; set; }
        /// <summary>
        /// Временная папка
        /// </summary>
        public string TempDirectory { get; set; }
        /// <summary>
        /// Путь к кластеру из окружения ноды
        /// </summary>
        public string ClusterUrl { get; set; }
		/// <summary>
		/// Путь к ноде из окружения кластера
		/// </summary>
		public string NodeUrl { get; set; }
		/// <summary>
		/// Количество акторов
		/// </summary>
		public int ActorsCount { get; set; }
    }
}

using Common.Messages;
using System.Threading.Tasks;
using Common.Models.Settings;
using Converter.Converters;
using Converter.Metadata;

namespace Common.Models
{
	/// <summary>
	/// Актор - основная рабочая единица системы
	/// </summary>
	public class Actor : BaseActor
	{
		/// <summary>
		/// Обработчик видео
		/// </summary>
		private readonly VideoProcessor _processor;
		
		public Actor(NodeSettings nodeSettings)
		{
			_processor = new VideoProcessor(nodeSettings.FfMpegPath, nodeSettings.TempDirectory);

			Receive<ProcessFileInfo>(file =>
			{
				var task = Task.Run(() =>
				{
					ProcessFile(file);
					StatusMessage message = new StatusMessage
					{
						TaskId = file.TaskId,
						Message = "Complete"
					};
					AddStatus(message);
					Callback(message);
				}, _cancellationToken);

				task.Wait();
			});
		}

		/// <summary>
		/// Запуск обработки файла
		/// </summary>
		void ProcessFile(ProcessFileInfo file)
		{
			_processor.ProcessVideo(file.FilePath, file.DestinationPath, VideoQuality.Vq240P, 24, VideoFormat.mp4);
		}
	}
}

using System;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Common.Utilities;
using Common.Messages;
using System.Collections.Generic;
using Common.Models.Settings;
using Common.Services.Interfaces;
using NLog;

namespace Common.Models
{
	/// <summary>
	/// Кластер 
	/// </summary>
	public class Cluster : BaseActor
	{
		#region private fields

		private readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private const string NODE_STATUS_OK = "OK";
		private const string NODE_STATUS_DISCONNECTED = "Disconnected";
		private IServiceProvider _serviceProvider { get; set; }

		private ConcurrentDictionary<string, NodeInfo> _allNodes = new ConcurrentDictionary<string, NodeInfo>();
		private ConcurrentDictionary<string, int> _availableNodes = new ConcurrentDictionary<string, int>();
		private ConcurrentDictionary<Guid, object> _outbox = new ConcurrentDictionary<Guid, object>();
		private ConcurrentDictionary<Guid, ProcessFileInfo> _files;

		private ConcurrentQueue<ProcessFileInfo> _filesToProcess;
		private ConcurrentDictionary<Guid, ProcessFileInfo> _processingFiles = new ConcurrentDictionary<Guid, ProcessFileInfo>();

		private int _lastIndex = 0;
		private long _allBytes = 0;
		private long _remain = 0;
		private long _speed = 0;
		private long _remainTime = 0;
		private DateTime _startDate = DateTime.UtcNow;

		private ClusterSettings _settings;
		#endregion

		#region public fields

		/// <summary>
		/// Подключенные ноды
		/// </summary>
		public ConcurrentDictionary<string, NodeInfo> AllNodes
		{
			get { return _allNodes; }
		}
		/// <summary>
		/// Полный объем всех файлов в обработку
		/// </summary>
		public long AllBytes
		{
			get { return _allBytes; }
		}
		/// <summary>
		/// Оставшийся объем
		/// </summary>
		public long Remain
		{
			get { return _remain; }
		}
		/// <summary>
		/// Оставшееся время
		/// </summary>
		public long RemainTime
		{
			get { return _remainTime; }
		}
		/// <summary>
		/// Скорость
		/// </summary>
		public long Speed
		{
			get { return _speed; }
		}
		#endregion

		#region .ctors

		public Cluster(ClusterSettings settings, IServiceProvider serviceProvider)
		{
			_settings = settings;
			_serviceProvider = serviceProvider;

			//Подключаем стартовавшие ноды
			Receive<NodeStarted>(ns =>
			{
				_availableNodes.TryAdd(ns.Host, ns.ActorsCount);
				_allNodes.TryAdd(ns.Host, new NodeInfo()
				{
					Name = ns.Host,
					Status = NODE_STATUS_OK,
					ActorsCount = ns.ActorsCount
				});

				// Выделяем задачи свежеподключенной ноде
				if (_filesToProcess != null)
				{
					for (int i = 0; i < ns.ActorsCount; i++)
					{
						ProcessNextFile(ns.Host);
					}
				}
			});

			//Обрабатываем хартбит ноды
			Receive<NodeReady>(nr =>
			{
				NodeInfo nodeInfo;
				_allNodes.TryGetValue(nr.Host, out nodeInfo);
				nodeInfo.LastReadyMessage = DateTime.UtcNow;
				_allNodes.TryUpdate(nr.Host, nodeInfo, nodeInfo);
			});

			//Удаляем отключаемые ноды
			Receive<NodeLeft>(nl =>
			{
				int actorsCount;
				_availableNodes.TryRemove(nl.Host, out actorsCount);

				NodeInfo nodeInfo;
				_allNodes.TryGetValue(nl.Host, out nodeInfo);

				if (nodeInfo == null)
					nodeInfo = new NodeInfo()
					{
						Name = nl.Host
					};

				nodeInfo.Status = NODE_STATUS_DISCONNECTED;
				_allNodes.AddOrUpdate(nl.Host, nodeInfo, (key, oldValue) => nodeInfo);
				ReProcessFiles(nl.Host);
			});

			//Обработка ошибок ноды
			Receive<NodeError>(ne =>
			{
				NodeInfo nodeInfo;
				_allNodes.TryGetValue(ne.Host, out nodeInfo);

				if (nodeInfo == null)
					nodeInfo = new NodeInfo()
					{
						Name = ne.Host
					};

				if (nodeInfo.Errors == null)
					nodeInfo.Errors = new List<string>();

				nodeInfo.Errors.Add(ne.Error);
				_allNodes.AddOrUpdate(ne.Host, nodeInfo, (key, oldValue) => nodeInfo);

				// Удаляем из обработки файлы, выделенные ноде
				ReProcessFiles(ne.Host);
			});

			//Обрабатываем завершение задачи
			Receive<TaskComplete>(tc =>
			{
				ProcessFileInfo file;

				if (!_processingFiles.TryRemove(tc.TaskId, out file) || file == null)
				{
					_logger.Error(Resource.ErrorNotFoundTask);
				}

				ReCalculateSpeed(file);
				ProcessNextFile(tc.Host);
			});
		}

		#endregion

		#region public methods

		/// <summary>
		/// На старте кластера сканируем хранилище и формируем список заданий
		/// </summary>
		public void Started()
		{
			_filesToProcess = FilesHelper.GetFilesToConvert(_settings);
			_allBytes = _filesToProcess.Select(fi => fi.Length).Sum();
			_remain = _allBytes;

			// Раздаем задачи всем нодам по количеству акторов
			foreach (var node in _availableNodes)
			{
				for (int i = 0; i < node.Value; i++)
				{
					ProcessNextFile(node.Key);
				}
			}
		}

		#endregion

		#region private methods

		/// <summary>
		/// Отправка ноде очередной задачи
		/// </summary>
		/// <param name="nodeHost">имя ноды</param>
		private void ProcessNextFile(string nodeHost)
		{
			try
			{
				var file = GetNextFile(nodeHost);
				var apiService = _serviceProvider.GetRequiredService<IApiService>();
				var url = string.Format("http://{0}/api/worker/processFile", nodeHost);
				_outbox.TryAdd(file.TaskId, file);
				apiService.PutOrPostRequest<ProcessFileInfo, string>(url, file);
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
			}
		}

		/// <summary>
		/// Получение следущего задания по обработке файла.назначение его ноде
		/// </summary>
		/// <param name="nodeHost"></param>
		/// <returns></returns>
		private ProcessFileInfo GetNextFile(string nodeHost)
		{
			ProcessFileInfo file;

			if (_filesToProcess.TryDequeue(out file))
			{
				file.Node = nodeHost;
				_processingFiles.TryAdd(file.TaskId, file);
			}
			return file;
		}

		/// <summary>
		/// Обработка файлов отключившейся ноды
		/// </summary>
		private void ReProcessFiles(string nodeHost)
		{
			var files = _processingFiles.Values.Where(pfi => pfi.Node == nodeHost).ToArray();
			ProcessFileInfo tempFile;

			foreach (var file in files)
			{
				_processingFiles.TryRemove(file.TaskId, out tempFile);
				_filesToProcess.Enqueue(tempFile);
			}
		}

		/// <summary>
		/// Пересчет скорости и прогноза по времени
		/// </summary>
		/// <param name="file"></param>
		private void ReCalculateSpeed(ProcessFileInfo file)
		{
			_remain = _remain - file.Length;
			TimeSpan ts = DateTime.UtcNow - _startDate;
			_speed = (_allBytes - _remain) / (int)ts.TotalSeconds;
			_remainTime = _remain / _speed;
		}

		#endregion
	}
}

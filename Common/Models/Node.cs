using System;
using System.Net;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using NLog;

using Common.Utilities;
using Common.Messages;
using Common.Models.Settings;
using Common.Services.Interfaces;

namespace Common.Models
{
	/// <summary>
	/// Нода - обработчик запросов от кластера и контейнер акторов
	/// </summary>
    public class Node
    {
        private const int _heartbeatInterval = 30;
		private IStatusService<StatusMessage> _status;
        private IServiceProvider _serviceProvider { get; set; }
        private string _hostUrl;
        private NodeSettings _nodeSettings;
		private readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public IServiceProvider ServiceProvider { set { _serviceProvider = value; } }

		/// <summary>
		/// Адрес ноды - необходим для обращения к ней в последующем
		/// </summary>
	    public string HostUrl
	    {
		    set
		    {
				var uri = new Uri(value);
			    _hostUrl = uri.Authority;
		    }
	    }

		/// <summary>
		/// .ctor
		/// Инициализируем акторы, добавляя обработчик завершения таска
		/// </summary>
		/// <param name="status"></param>
		/// <param name="nodeSettings"></param>
        public Node(IStatusService<StatusMessage> status, NodeSettings nodeSettings)
        {
            _status = status;
            _nodeSettings = nodeSettings;
            
            for (var i = 0; i < nodeSettings.ActorsCount; i++)
            {
                var actor = new Actor(nodeSettings);
                actor.AddCallback("node", message =>
                {
                    var apiService = _serviceProvider.GetRequiredService<IApiService>();
                    var url = string.Format("{0}/api/worker/taskComplete", _nodeSettings.ClusterUrl);
                    apiService.PutOrPostRequest<TaskComplete, string>(url, new TaskComplete()
                    {
                        TaskId = message.TaskId,
                        Host = _hostUrl
					});
                });

				_status.AddActor(Guid.NewGuid().ToString(), actor);
            }
        }

		/// <summary>
		/// Обработчик старта ноды
		/// </summary>
        public void Started()
        {
            _status.AddCallback("test", action =>
            {
                if (action.PayLoad == null)
                {
                    _status.Next.Send(action.Message);
                }
                else
                {
                    _status.Next.Send(action.PayLoad);
                }
            });

            var apiService = _serviceProvider.GetRequiredService<IApiService>();

            // Отправляем информацию о старте и количестве акторов в ноде
            try
            {
                var url = string.Format("{0}/api/worker/started", _nodeSettings.ClusterUrl);
                apiService.PutOrPostRequest<NodeStarted, string>(url, new NodeStarted()
                {
                    ActorsCount = _nodeSettings.ActorsCount,
                    Host = _hostUrl
				});
            }
            catch (Exception ex)
            {
	            SendError(ex.ToString());
				_logger.Error(ex);
			}
            
            var cancellationTokenSource = new CancellationTokenSource();

            // Добавил простой heartbeat чтобы отслеживать на кластере существование нод
            TaskRepeater.Interval(TimeSpan.FromSeconds(_heartbeatInterval), () =>
            {
                try
                {
                    apiService.GetRequest<string>(string.Format("{0}/api/worker/ready/{1}", _nodeSettings.ClusterUrl, _hostUrl));
                }
                catch (Exception ex)
                {
					SendError(ex.ToString());
					_logger.Error(ex);
				}
            }, cancellationTokenSource.Token, true);
        }

		/// <summary>
		/// Обработчик остановки сервиса ноды
		/// </summary>
        public void Stopped()
        {
            try
            {
                var apiService = _serviceProvider.GetRequiredService<IApiService>();
                apiService.GetRequest<string>(string.Format("{0}/api/worker/left/{1}", _nodeSettings.ClusterUrl, WebUtility.HtmlEncode(_hostUrl)));
            }
            catch (Exception ex)
            {
				SendError(ex.ToString());
				_logger.Error(ex);
            }
        }

		/// <summary>
		/// Обработчик ошибки ноды 
		/// В продакшн нужно развить типизацию вх. параметров
		/// </summary>
		/// <param name="error">Текстовое описание ошибки</param>
		public void SendError(string error)
		{
			try
			{
				var apiService = _serviceProvider.GetRequiredService<IApiService>();
				var url = string.Format("{0}/api/worker/error", _nodeSettings.ClusterUrl);
				apiService.PutOrPostRequest<NodeError, string>(url, new NodeError()
				{
					Error = error,
					Host = _hostUrl
				});
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
			}
		}
	}
}

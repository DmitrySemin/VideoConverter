using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Messages;
using Common.Models.Interfaces;

namespace Common.Models
{
	/// <summary>
	/// Базовый класс актора, инкапсулирует хранение и обработку сообщений
	/// </summary>
	public abstract class BaseActor : IActor<StatusMessage>
	{
		#region private fields

		/// <summary>
		/// Максимальный размер очереди на обработку сообщений
		/// </summary>
		private const int _maxMessages = 10;
		/// <summary>
		/// Идентификатор актора
		/// </summary>
		private string _id;
		/// <summary>
		/// Признак блокировки актора
		/// </summary>
		private bool _isBusy;
		/// <summary>
		/// Маркёр отмены поставленной задачи
		/// </summary>
		protected CancellationToken _cancellationToken;
		/// <summary>
		/// Очередь сообщений
		/// </summary>
		private ConcurrentQueue<StatusMessage> _queue = new ConcurrentQueue<StatusMessage>();
		/// <summary>
		/// Обработчики
		/// </summary>
		private ConcurrentDictionary<Type, Action<object>> _handlers = new ConcurrentDictionary<Type, Action<object>>();
		/// <summary>
		/// Экшны
		/// </summary>
		private ConcurrentDictionary<string, Action<StatusMessage>> _actions = new ConcurrentDictionary<string, Action<StatusMessage>>();
		/// <summary>
		/// Очередь на обработку сообщений
		/// </summary>
		private ConcurrentQueue<object> _mailbox = new ConcurrentQueue<object>();
		
		private object _lockObj = new object();
		private object _lockCheck = new object();

		#endregion

		#region public fields

		/// <summary>
		/// Идентификатор актора
		/// </summary>
		public string Id
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}
		/// <summary>
		/// Маркёр отмены поставленной задачи
		/// </summary>
		public CancellationToken CancellationToken
		{
			get
			{
				return _cancellationToken;
			}
			set
			{
				_cancellationToken = value;
			}
		}
		/// <summary>
		/// Текущее сообщение в обработке 
		/// </summary>
		public StatusMessage CurrentStatus
		{
			get
			{
				return _queue.LastOrDefault();
			}
		}
		/// <summary>
		/// Потокобезопасная реализация блокировки
		/// </summary>
		public bool IsBusy
		{
			get
			{
				lock (_lockObj)
				{
					return _isBusy;
				}
			}
			set
			{
				lock (_lockObj)
				{
					_isBusy = value;
				}
			}
		}

		#endregion

		#region public methods
		/// <summary>
		/// Добавление экшна
		/// </summary>
		public void AddCallback(string id, Action<StatusMessage> action)
		{
			RemoveCallback(id);
			_actions.TryAdd(id, action);
		}
		/// <summary>
		/// Удаление экшна
		/// </summary>
		public void RemoveCallback(string id)
		{
			if (_actions.ContainsKey(id))
			{
				Action<StatusMessage> outAction;
				_actions.TryRemove(id, out outAction);
			}
		}
		/// <summary>
		/// Постановка задачи в очередь
		/// </summary>
		public void AddStatus(StatusMessage message)
		{
			_queue.Enqueue(message);

			StatusMessage outItem;
			while (_queue.Count > _maxMessages)
			{
				_queue.TryDequeue(out outItem);
			}
		}
		/// <summary>
		/// Вызов экшна (например обработка завершения задачи)
		/// </summary>
		public void Callback(StatusMessage message)
		{
			if (_actions.Count > 0)
			{
				foreach (var action in _actions)
				{
					action.Value.Invoke(message);
				}
			}
		}

		#endregion

		#region private methods
		/// <summary>
		/// Инициализация обработчиков полученных сообщений 
		/// </summary>
		protected void Receive<V>(Action<V> handler, Predicate<V> execute = null)
		{
			var handlerObj = new Action<object>(
				obj =>
				{
					var castObj = (V)Convert.ChangeType(obj, typeof(V));
					handler(castObj);
				});
			_handlers.TryAdd(typeof(V), handlerObj);
		}
		/// <summary>
		/// Отправка сообщения актору
		/// </summary>
		/// <typeparam name="V">Тип сообщения</typeparam>
		/// <param name="message">Сообщение</param>
		public void Send<V>(V message)
		{
			_mailbox.Enqueue(message);
			MessageFinished();
		}
		/// <summary>
		/// Обработка завершения сообщения
		/// </summary>
		private void MessageFinished()
		{
			Action finished = () => MessageFinished();
			ProcessNextMessage(finished);
		}

		/// <summary>
		/// Обработка сообщения
		/// </summary>
		/// <param name="finished"></param>
		private void ProcessNextMessage(Action finished)
		{
			lock (_lockCheck)
			{
				if (_mailbox.Count > 0 && _handlers.Count > 0 && !IsBusy)
				{
					IsBusy = true;
					object message = null;
					_mailbox.TryDequeue(out message);
					Action<object> handler = null;
					var key = message.GetType();
					_handlers.TryGetValue(key, out handler);
					
					var task = new Task(() =>
					{
						try
						{
							handler(message);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex);
						}
						finally
						{
							IsBusy = false;
							finished();
						}
					}, _cancellationToken);
					task.Start();
				}
			}
		}

		#endregion
	}
}

using System;
using System.Collections.Concurrent;
using System.Linq;
using Common.Models;
using Common.Models.Interfaces;
using Common.Services.Interfaces;

namespace Common.Services
{
	/// <summary>
	/// Имплементация хранилища и обработчика акторов 
	/// </summary>
    public class StatusService<T> : IStatusService<T> where T : class
    {
        private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private ConcurrentDictionary<string, IActor<T>> _actors = new ConcurrentDictionary<string, IActor<T>>();
        private ConcurrentDictionary<string, Action<T>> _actions = new ConcurrentDictionary<string, Action<T>>();
        private int _lastIndex = 0;

        public IActor<T> Next
        {
            get
            {
                if ((_actors?.Count ?? 0) == 0)
                {
                    return null;
                }
                
                if (_lastIndex == _actors.Count - 1)
                {
                    _lastIndex = 0;
                }
                else
                {
                    _lastIndex++;
                }

                return _actors.ElementAt(_lastIndex).Value;
            }
        }

		public T CurrentStatus
		{
			get
			{
				return StatusMessages.LastOrDefault();
			}
		}

        public ConcurrentQueue<T> StatusMessages { get { return _queue; } }
        public int MaxMessages { get; set; } = 10;


		public void AddStatus(T message)
        {
            _queue.Enqueue(message);

            T outItem;

            while (_queue.Count > this.MaxMessages)
            {
                _queue.TryDequeue(out outItem);
            }

            T dequeuedMessage = this.CurrentStatus;

            if (_actions.Count > 0)
            {
                foreach (var action in _actions)
                {
                    action.Value.Invoke(dequeuedMessage);
                }
            }
        }

		/// <summary>
		/// Добавление обработчика
		/// </summary>
        public void AddCallback(string id, Action<T> action)
        {
            RemoveCallback(id);
            _actions.TryAdd(id, action);
        }
		
		/// <summary>
		/// Обработчик удаления экшна
		/// </summary>
        public void RemoveCallback(string id)
        {
            if (_actions.ContainsKey(id))
            {
                Action<T> outAction;
                _actions.TryRemove(id, out outAction);
            }
        }

        public void Dispose()
        {

        }

		/// <summary>
		/// Удаление актора
		/// </summary>
        public void RemoveActor(string id)
        {
            if (_actors.ContainsKey(id))
            {
                IActor<T> outActor;
	            _actors.TryRemove(id, out outActor);
            }
        }
		/// <summary>
		/// Добавление актора
		/// </summary>
        public void AddActor(string id, IActor<T> actor)
        {
            RemoveActor(id);
            _actors.TryAdd(id, actor);
        }
    }
}

using System;
using System.Threading;

namespace Common.Models.Interfaces
{
	public interface IActor<T>
	{
		/// <summary>
		/// Идентификатор актора
		/// </summary>
		string Id { get; set; }
		/// <summary>
		/// Маркёр отмены поставленной задачи
		/// </summary>
		CancellationToken CancellationToken { get; set; }
		/// <summary>
		/// Постановка задачи в очередь
		/// </summary>
		void AddStatus(T message);
		/// <summary>
		/// Вызов экшна
		/// </summary>
		void Callback(T message);
		/// <summary>
		/// Добавление экшна
		/// </summary>
		void AddCallback(string id, Action<T> action);
		/// <summary>
		/// Удаление экшна
		/// </summary>
		void RemoveCallback(string id);
		/// <summary>
		/// Отправка сообщения актору
		/// </summary>
		/// <typeparam name="V">Тип сообщения</typeparam>
		/// <param name="message">Сообщение</param>
		void Send<V>(V message);
	}
}

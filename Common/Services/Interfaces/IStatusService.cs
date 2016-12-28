using System;
using System.Collections.Concurrent;
using Common.Models;
using Common.Models.Interfaces;

namespace Common.Services.Interfaces
{
    public interface IStatusService<T> : IDisposable where T : class
    {
        T CurrentStatus { get; }
        ConcurrentQueue<T> StatusMessages { get; }
        int MaxMessages { get; set; }

        void AddStatus(T message);
        void AddCallback(string id, Action<T> action);
        void RemoveCallback(string id);

        void RemoveActor(string id);
        void AddActor(string id, IActor<T> actor);

        IActor<T> Next { get; }
    }
}

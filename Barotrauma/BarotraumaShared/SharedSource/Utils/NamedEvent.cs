using System;
using System.Collections.Generic;

namespace Barotrauma
{
    internal sealed class NamedEvent<T> : IDisposable
    {
        private readonly Dictionary<Identifier, Action<T>> events = new Dictionary<Identifier, Action<T>>();

        public void Register(Identifier identifier, Action<T> action)
        {
            if (HasEvent(identifier))
            {
                throw new ArgumentException($"Event with the identifier \"{identifier}\" has already been registered.", nameof(identifier));
            }

            events.Add(identifier, action);
        }

        public void RegisterOverwriteExisting(Identifier identifier, Action<T> action)
        {
            if (HasEvent(identifier))
            {
                Deregister(identifier);
            }

            Register(identifier, action);
        }

        public void Deregister(Identifier identifier)
        {
            events.Remove(identifier);
        }

        public void TryDeregister(Identifier identifier)
        {
            if (!HasEvent(identifier)) { return; }
            Deregister(identifier);
        }

        public bool HasEvent(Identifier identifier) => events.ContainsKey(identifier);

        public void Invoke(T data)
        {
            foreach (var (_, action) in events)
            {
                action?.Invoke(data);
            }
        }

        public void Dispose()
        {
            events.Clear();
        }
    }
}
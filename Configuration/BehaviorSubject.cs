using System;
using System.Collections.Concurrent;
using Its.Recipes;

namespace Its.Configuration
{
    internal class BehaviorSubject<T> : IObserver<T>, IObservable<T>, IDisposable
    {
        private ConcurrentBag<IObserver<T>> observers = new ConcurrentBag<IObserver<T>>();
        private T currentValue;

        public BehaviorSubject(T initialValue)
        {
            currentValue = initialValue;
        }

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public void OnNext(T value)
        {
            currentValue = value;
            observers.ForEach(o => o.OnNext(currentValue));
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            observers.ForEach(o => o.OnError(error));
            Dispose();
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
            Dispose();
        }

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <returns>
        /// The observer's interface that enables resources to be disposed.
        /// </returns>
        /// <param name="observer">The object that is to receive notifications.</param>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            observers.Add(observer);
            observer.OnNext(currentValue);
            return new AnonymousDisposable(() => observers.TryTake(out observer));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            currentValue = default (T);
            observers = new ConcurrentBag<IObserver<T>>();
        }
    }
}
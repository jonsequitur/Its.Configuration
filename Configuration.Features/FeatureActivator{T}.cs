// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Its.Configuration.Features
{
    /// <summary>
    /// Manages activate and deactivate calls based on observers and the availability of dependencies.
    /// </summary>
    public class FeatureActivator<T> : IObservable<T>, IDisposable
    {
        private readonly Func<T> activate;
        private readonly Func<T> deactivate;
        private readonly IConnectableObservable<T> available;
        private IDisposable connection;
        private bool hasBeenActivated = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureActivator"/> class.
        /// </summary>
        /// <param name="activate">A function to be called when the first observer subscribes and all dependencies (if any) are available.</param>
        /// <param name="deactivate">A function to be called when any dependency becomes unavailable.</param>
        /// <param name="dependsOn">A set of dependencies, all of which must produce a true result in order for activation to be triggered.</param>
        public FeatureActivator(Func<T> activate, Func<T> deactivate, params IObservable<bool>[] dependsOn)
        {
            if (activate == null)
            {
                throw new ArgumentNullException("activate");
            }
            this.activate = activate;
            this.deactivate = deactivate ?? (() => default(T));

            IObservable<bool> dependenciesAvailable = new BehaviorSubject<bool>(true);

            foreach (var dependency in dependsOn)
            {
                dependenciesAvailable = dependenciesAvailable.CombineLatest(dependency, (x, y) => x && y);
            }

            available = dependenciesAvailable
                .DistinctUntilChanged()
                .Select(availableNow =>
                {
                    if (!availableNow)
                    {
                        if (hasBeenActivated)
                        {
                            return Deactivate();
                        }
                        return default(T);
                    }

                    return Activate();
                })
                .Replay(1);
        }

        internal T Activate()
        {
            hasBeenActivated = true;
            return activate();
        }

        internal T Deactivate()
        {
            return deactivate();
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
            connection = available.Connect();
            return available.Subscribe(observer);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
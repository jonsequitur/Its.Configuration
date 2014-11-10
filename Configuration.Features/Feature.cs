using System;
using System.Linq;

namespace Its.Configuration.Features
{
    /// <summary>
    /// Describes a feature.
    /// </summary>
    public abstract class Feature : IFeature
    {
        internal Feature()
        {
        }

        /// <summary>
        /// Gets the name of the feature.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Produces a non-terminating observable representing the available state over time.
        /// </summary>
        /// <remarks>Calls to an instance's methods while it is unavailable can result in an <see cref="InvalidOperationException" />. Observers will receive an <see cref="IObserver{T}.OnCompleted" /> call if the state of the feature will never change at runtime. In the event that the instance become faulted, they will receive a <see cref="IObserver{T}.OnError" /> call.</remarks>
        public abstract IObservable<bool> Availability { get; }
    }
}
// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Its.Configuration.Features
{
    /// <summary>
    /// Indicates the availability of the functionality exposed by an implementing class.
    /// </summary>
    public interface IFeature
    {
        /// <summary>
        /// Produces a non-terminating observable representing the available state over time.
        /// </summary>
        /// <remarks>Calls to an instance's methods while it is unavailable can result in an <see cref="InvalidOperationException" />. Observers will receive an <see cref="IObserver{T}.OnCompleted" /> call if the state of the feature will never change at runtime. In the event that the instance become faulted, they will receive a <see cref="IObserver{T}.OnError" /> call.</remarks>
        IObservable<bool> Availability { get; }
    }
}
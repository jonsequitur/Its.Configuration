// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reactive.Linq;

namespace Its.Configuration.Features
{
    /// <summary>
    ///     A feature that can be turned on and off at runtime.
    /// </summary>
    public abstract class OnOffFeature : IFeature
    {
        private readonly FeatureActivator activator;
        private readonly BehaviorSubject<bool> availability;

        protected OnOffFeature(bool @on)
        {
            availability = new BehaviorSubject<bool>(@on);
            activator = new FeatureActivator(
                Activate,
                Deactivate,
                availability);
        }

        protected abstract void Activate();

        protected abstract void Deactivate();

        protected bool IsAvailable
        {
            get
            {
                return availability.Last();
            }
            set
            {
                availability.OnNext(value);
            }
        }

        public IObservable<bool> Availability
        {
            get
            {
                return activator;
            }
        }
    }
}
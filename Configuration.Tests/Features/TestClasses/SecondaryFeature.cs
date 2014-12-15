// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using Its.Configuration.Features;

namespace Its.Configuration.Tests.Features.TestClasses
{
    [Export(typeof (IFeature))]
    public class SecondaryFeature : IFeature
    {
        public static int CtorCount = 0;
        public static int ActivateCount = 0;
        private readonly IObservable<bool> availability;
        public static Action<PrimaryFeature> OnCtor = feature => { };
        public static Action OnActivate = () => { };

        public SecondaryFeature(PrimaryFeature primaryFeature)
        {
            availability = new FeatureActivator(
                Activate, 
                Deactivate, 
                dependsOn: primaryFeature.Availability);
            Interlocked.Increment(ref CtorCount);
            OnCtor(primaryFeature);
        }

        private void Deactivate()
        {
        }

        private void Activate()
        {
            OnActivate();
        }

        public IObservable<bool> Availability
        {
            get
            {
                return availability;
            }
        }
    }
}
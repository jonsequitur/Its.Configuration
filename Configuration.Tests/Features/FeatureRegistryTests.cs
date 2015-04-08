// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Its.Configuration.Features;
using Its.Configuration.Tests.Features.TestClasses;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Its.Configuration.Tests.Features
{
    [TestFixture]
    public class FeatureRegistryTests
    {
        [SetUp]
        public void SetUp()
        {
            PrimaryFeature.CtorCount = 0;
            PrimaryFeature.ActivateCount = 0;
            PrimaryFeature.OnCtor = () => { };
            PrimaryFeature.OnActivate = () => { };

            SecondaryFeature.CtorCount = 0;
            SecondaryFeature.ActivateCount = 0;
            SecondaryFeature.OnCtor = primary => { };
            SecondaryFeature.OnActivate = () => { };

            DuckTypedFeature.CtorCount = 0;
            DuckTypedFeature.ActivateCount = 0;
            DuckTypedFeature.OnCtor = () => { };
            DuckTypedFeature.OnActivate = () => { };
        }

        [Test]
        public void A_feature_can_be_retrieved_from_the_registry_as_an_observable()
        {
            var registry = new FeatureRegistry
            {
                typeof (PrimaryFeature)
            };

            Assert.That(
                registry.Get<PrimaryFeature>(),
                Is.InstanceOf<IObservable<PrimaryFeature>>());
        }

        [Test]
        public void When_a_feature_is_not_registered_it_returns_an_empty_and_uncompleted_observable()
        {
            var registry = new FeatureRegistry();
            var initialized = false;

            registry
                .Get<SecondaryFeature>()
                .Timeout(TimeSpan.FromSeconds(2))
                .Subscribe(f => { initialized = true; }, ex => { });

            Assert.That(initialized, Is.False);
        }

        [Test]
        public void When_a_feature_is_subscribed_before_it_is_registered_then_subscriber_is_notified_upon_registration()
        {
            var barrier = new Barrier(2);
            var initialized = false;
            var registry = new FeatureRegistry();
            PrimaryFeature.OnActivate = barrier.SignalAndWait;

            registry
                .Get<SecondaryFeature>()
                .Timeout(TimeSpan.FromSeconds(2))
                .Subscribe(f => { initialized = true; }, ex => { });

            Assert.That(initialized, Is.False);

            registry.Add(r => new SecondaryFeature(new PrimaryFeature()));

            barrier.SignalAndWait(2000);

            Assert.That(initialized, Is.True);
        }

        [Test]
        public void A_feature_can_be_subscribed_before_it_is_registered_and_when_it_is_later_initialized_subscribers_will_be_notified()
        {
            string result = null;
            var feature = new Feature<string>();

            feature.Subscribe(s => result = s);

            Assert.That(result, Is.Null);

            feature.OnNext("hello");

            Assert.That(result, Is.EqualTo("hello"));
        }

        [Test]
        public void Optional_features_can_throw_during_initialization_without_causing_other_features_to_fail_to_load()
        {
            SecondaryFeature.OnCtor = f => { throw new Exception("oops!"); };

            var registry = new FeatureRegistry()
                .Add<SecondaryFeature>()
                .Add<PrimaryFeature>();

            var initialized = false;

            registry
                .Get<SecondaryFeature>()
                .Timeout(TimeSpan.FromSeconds(2))
                .Subscribe(f => { initialized = true; }, ex => { });

            Assert.That(initialized, Is.False);
        }

        [Test]
        public void When_a_feature_fails_to_load_the_error_is_published_to_OnError_and_not_thrown()
        {
            var feature = new Feature<string>(() => { throw new ArgumentException("oops!"); });

            Exception error = null;
            string result = null;
            feature.Subscribe(f => result = f, ex => error = ex);

            Assert.That(result, Is.Null);
            Assert.That(error, Is.InstanceOf<ArgumentException>());
        }
    }
}
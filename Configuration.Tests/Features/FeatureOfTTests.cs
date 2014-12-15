// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using FluentAssertions;
using Its.Configuration.Features;
using Its.Configuration.Tests.Features.TestClasses;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Its.Configuration.Tests.Features
{
    [TestFixture]
    public class FeatureOfTTests
    {
        [Test]
        public void Feature_starts_in_an_unavailable_state_if_no_instance_is_provided()
        {
            var feature = new Feature<string>();

            var scheduler = new TestScheduler();
            bool wasAvailable = false;
            feature.Availability.Timeout(TimeSpan.FromMinutes(5), scheduler).Subscribe(i => wasAvailable = i, ex => { });

            scheduler.AdvanceBy(TimeSpan.FromDays(1).Ticks);

            Assert.That(wasAvailable, Is.False);
        }

        [Test]
        public void After_it_is_subscribed_a_Feature_is_available_if_an_initialization_function_is_supplied()
        {
            var feature = new Feature<string>(() => "hello");

            feature.Subscribe(f => { });

            Assert.That(feature.Availability.First(), Is.True);
        }

        [Test]
        public void Feature_observable_returns_feature_from_Func_ctor()
        {
            var feature = new Feature<string>(() => "hello");

            feature.Subscribe(v => { });

            Assert.That(feature.First(), Is.EqualTo("hello"));
        }

        [Test]
        public void Feature_observable_returns_feature_from_object_ctor()
        {
            var feature = new Feature<string>((object) "hello");

            feature.Subscribe(v => { });

            Assert.That(feature.First(), Is.EqualTo("hello"));
        }

        [Test]
        public void Feature_observable_returns_feature_from_object_ctor_when_passed_a_delegate()
        {
            Func<object> getter = () => "hello";
            var feature = new Feature<string>(getter);

            feature.Subscribe(v => { });

            Assert.That(feature.First(), Is.EqualTo("hello"));
        }

        [Test]
        public void Feature_initialization_can_time_out()
        {
            string result = null;
            var feature = new Feature<string>(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                return "hi";
            });

            var featureWithTimeout = feature.Timeout(TimeSpan.FromSeconds(.1));

            featureWithTimeout.Subscribe(s => result = s, ex => { /* ignore the TimeoutException */ });

            Assert.That(result, Is.Null);
        }

        [Test]
        public void A_Feature_whose_instance_is_an_IFeature_is_unavailable_until_the_IFeature_is_available()
        {
            var inner = new Feature<string>();
            var outer = new Feature<Feature<string>>(inner);

            bool available = false;
            outer.Availability.Subscribe(a => available = a);

            Assert.That(available, Is.False);

            inner.OnNext("hello");

            Assert.That(available, Is.True);
        }

        [Test]
        public void A_Feature_whose_instance_is_an_IFeature_produces_a_value_when_the_IFeature_is_available()
        {
            var inner = new Feature<string>();
            var outer = new Feature<Feature<string>>(inner);
            Feature<string> result = null;

            outer.Subscribe(v => result = v);

            inner.OnNext("hello");

            Assert.That(outer.Availability.First(), Is.EqualTo(true));
            Assert.That(outer.First().First(), Is.EqualTo("hello"));
        }

        [Test]
        public void A_Feature_has_a_name_based_on_the_type()
        {
            var feature = new Feature<string>();

            Assert.That(feature.Name, Is.EqualTo("System.String"));
        }
        
        [Test]
        public void A_Feature_name_can_be_overridden_using_DisplayNameAttribute()
        {
            var feature = new Feature<SomethingWithDisplayName>();

            Assert.That(feature.Name, Is.EqualTo("hello"));
        }

        [Test]
        public void When_no_availability_has_been_signaled_then_IsAvailable_is_false()
        {
            var subject = new ReplaySubject<bool>();
            var feature = new Feature<ActivatableFeature>(new ActivatableFeature(subject));

            Assert.That(feature.IsAvailable(), Is.False);
        }

        [Test]
        public void When_false_availability_has_been_signaled_then_IsAvailable_is_false()
        {
            var feature = new Feature<ActivatableFeature>(new ActivatableFeature(Observable.Return(false)));

            Assert.That(feature.IsAvailable(), Is.False);
        }

        [Test]
        public void When_true_availability_has_been_signaled_then_IsAvailable_is_true()
        {
            var feature = new Feature<ActivatableFeature>(new ActivatableFeature(Observable.Return(true)));

            Assert.That(feature.IsAvailable(), Is.True);
        }

        [Test]
        public void When_wrapped_class_duck_types_IFeature_then_its_Availability_is_mirrored_by_Feature_of_Ts_Availability()
        {
            var feature = new Feature<DuckTypedFeature>(() => new DuckTypedFeature());

            DuckTypedFeature.ActivateCount.Should().Be(0);

            feature.Availability
                .SubscribeOn(Scheduler.Immediate)
                .Subscribe();

            DuckTypedFeature.ActivateCount.Should().Be(1);
        }

        [DisplayName("hello")]
        private class SomethingWithDisplayName
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Its.Configuration.Features;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Its.Configuration.Tests.Features
{
    [TestFixture]
    public class FeatureActivatorTests
    {
        private CompositeDisposable disposables;

        [SetUp]
        public void SetUp()
        {
            disposables = new CompositeDisposable();
        }

        [TearDown]
        public void TearDown()
        {
            disposables.Dispose();
        }

        [Test]
        public void Is_available_by_default()
        {
            var activated = false;
            var activator = new FeatureActivator(() => { activated = true; });
            Assert.That(activator.First(), Is.True);
            Assert.That(activated);
        }

        [Test]
        public void Is_not_initially_available_if_dependency_returns_false()
        {
            var activator = new FeatureActivator(() => { }, dependsOn: Observable.Return(false));
            Assert.That(activator.First(), Is.False);
        }

        [Test]
        public void Produces_a_value_each_time_the_aggregate_availability_of_dependencies_changes()
        {
            var dependency = new BehaviorSubject<bool>(false);
            var activator = new FeatureActivator(() => { }, dependsOn: dependency);

            var notifications = new List<bool>();

            disposables.Add(activator.Subscribe(notifications.Add));

            dependency.OnNext(false);
            dependency.OnNext(false);
            dependency.OnNext(true);
            dependency.OnNext(true);
            dependency.OnNext(true);
            dependency.OnNext(true);
            dependency.OnNext(false);
            dependency.OnNext(false);
            dependency.OnNext(false);

            Assert.That(notifications.IsSameSequenceAs(false, true, false));
        }

        [Test]
        public void Activate_is_not_called_before_subscribe()
        {
            var activations = 0;

            new FeatureActivator(() => activations++);

            Assert.That(activations, Is.EqualTo(0));
        }

        [Test]
        public void Activate_is_not_called_more_than_once_during_concurrent_calls_when_activator_has_no_dependencies()
        {
            var activations = 0;
            var notifications = 0;
            var barrier = new Barrier(2);

            var activator = new FeatureActivator(() =>
            {
                Interlocked.Increment(ref activations);
                barrier.SignalAndWait();
            });

            for (var i = 0; i < 10; i++)
            {
                disposables.Add(activator
                                    .ObserveOn(NewThreadScheduler.Default)
                                    .SubscribeOn(NewThreadScheduler.Default)
                                    .Subscribe(n =>
                                    {
                                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                                        Interlocked.Increment(ref notifications);
                                    }));
            }

            // give both subscribers enough time to make sure one has advanced to the activator barrier
            Thread.Sleep(500);
            barrier.SignalAndWait();
            // give them enough time to propagate their notifications
            Thread.Sleep(500);

            Assert.That(activations, Is.EqualTo(1));
            Assert.That(notifications, Is.EqualTo(10));
        }

        [Test]
        public void Activate_is_not_called_more_than_once_during_concurrent_calls_when_activator_has_dependencies()
        {
            var activations = 0;
            var notifications = 0;
            var barrier = new Barrier(2);
            var subject = new BehaviorSubject<bool>(false);

            var activator = new FeatureActivator(() =>
            {
                Console.WriteLine("activated!");
                Interlocked.Increment(ref activations);
                barrier.SignalAndWait();
            }, dependsOn: subject);

            for (var i = 0; i < 10; i++)
            {
                disposables.Add(activator
                                    .ObserveOn(NewThreadScheduler.Default)
                                    .SubscribeOn(NewThreadScheduler.Default)
                                    .Subscribe(n =>
                                    {
                                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                                        Interlocked.Increment(ref notifications);
                                    }));
            }

            new Thread(() => subject.OnNext(true)).Start();

            // give both subscribers enough time to make sure one has advanced to the activator barrier
            Thread.Sleep(1000);
            barrier.SignalAndWait(3000);
            // give them enough time to propagate their notifications
            Thread.Sleep(1000);

            Assert.That(notifications, Is.AtLeast(10)); // sanity check
            Assert.That(activations, Is.EqualTo(1));
        }

        [Test]
        public void When_there_are_multiple_subscribers_then_activate_is_not_called_repeatedly()
        {
            var activations = 0;

            var activator = new FeatureActivator(() => activations++);

            disposables.Add(activator.Subscribe(_ => { }));
            disposables.Add(activator.Subscribe(_ => { }));
            disposables.Add(activator.Subscribe(_ => { }));

            Assert.That(activations, Is.EqualTo(1));
        }

        [Test]
        public void Activate_is_not_called_until_dependencies_return_true()
        {
            var activations = 0;
            var notifications = new List<bool>();
            var subject = new BehaviorSubject<bool>(false);

            var activator = new FeatureActivator(() => activations++, dependsOn: subject);

            disposables.Add(activator.Subscribe(notifications.Add));

            Assert.That(activations, Is.EqualTo(0));
            Assert.That(notifications.IsSameSequenceAs(false));

            subject.OnNext(false);

            Assert.That(activations, Is.EqualTo(0));
            Assert.That(notifications.IsSameSequenceAs(false));

            subject.OnNext(true);

            Assert.That(activations, Is.EqualTo(1));
            Assert.That(notifications.IsSameSequenceAs(false, true));

            disposables.Add(activator.Subscribe(notifications.Add));

            Assert.That(activations, Is.EqualTo(1));
            Assert.That(notifications.IsSameSequenceAs(false, true, true));
        }

        [Test]
        public void When_there_are_multiple_dependencies_they_must_all_return_true_before_Activate_is_called()
        {
            var activations = 0;
            var dependency1 = new BehaviorSubject<bool>(false);
            var dependency2 = new BehaviorSubject<bool>(false);
            var activator = new FeatureActivator(
                activate: () => activations++,
                dependsOn: new[] { dependency1, dependency2 });

            var notifications = new List<bool>();

            disposables.Add(activator.Subscribe(notifications.Add));

            Assert.That(activations, Is.EqualTo(0));
            Assert.That(notifications.IsSameSequenceAs(false));

            dependency1.OnNext(true);

            Assert.That(activations, Is.EqualTo(0));
            Assert.That(notifications.IsSameSequenceAs(false));

            dependency2.OnNext(true);

            Assert.That(activations, Is.EqualTo(1));
            Assert.That(notifications.IsSameSequenceAs(false, true));
        }

        [Test]
        public void When_any_dependency_produces_false_then_deactivate_is_called()
        {
            var deactivations = 0;
            var dependency1 = new BehaviorSubject<bool>(true);
            var dependency2 = new BehaviorSubject<bool>(true);
            var activator = new FeatureActivator(
                activate: () => { },
                deactivate: () => deactivations++,
                dependsOn: new[] { dependency1, dependency2 });

            var notifications = new List<bool>();

            disposables.Add(activator.Subscribe(notifications.Add));

            Assert.That(deactivations, Is.EqualTo(0));
            Assert.That(notifications.IsSameSequenceAs(true));

            dependency1.OnNext(false);

            Assert.That(deactivations, Is.EqualTo(1));
            Assert.That(notifications.IsSameSequenceAs(true, false));
        }

        [Test]
        public void Deactivate_is_not_called_until_after_activate_has_been_called()
        {
            var deactivations = 0;
            var subject = new BehaviorSubject<bool>(false);
            var activator = new FeatureActivator(() => { }, () => deactivations++, dependsOn: subject);
            disposables.Add(activator.Subscribe(_ => { }));

            subject.OnNext(true);

            Assert.That(deactivations, Is.EqualTo(0));

            subject.OnNext(false);

            Assert.That(deactivations, Is.EqualTo(1));
        }
    }
}
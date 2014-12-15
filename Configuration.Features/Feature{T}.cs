// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Its.Recipes;

namespace Its.Configuration.Features
{
    /// <summary>
    ///     Wraps a feature implementation in order to provide implementation replaceability and lazy instantiation.
    /// </summary>
    public sealed class Feature<T> :
        Feature,
        IObserver<T>,
        IObservable<T> 
        where T : class
    {
        private readonly ReplaySubject<T> currentValue = new ReplaySubject<T>(1);

        private Lazy<T> lazyInitialize;

        private readonly string name = typeof (T)
            .GetCustomAttributes(false)
            .OfType<DisplayNameAttribute>()
            .FirstOrDefault()
            .IfNotNull()
            .Then(att => att.DisplayName)
            .Else(() => AttributedModelServices.GetContractName(typeof (T)));

        /// <summary>
        ///     Initializes a new instance of the <see cref="Feature&lt;T&gt;" /> class.
        /// </summary>
        public Feature()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Feature&lt;T&gt;" /> class.
        /// </summary>
        /// <param name="initialValue">The initial value for the feature instance.</param>
        public Feature(T initialValue) : this()
        {
            currentValue.OnNext(initialValue);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Feature&lt;T&gt;" /> class.
        /// </summary>
        /// <param name="lazyInitialValue">
        ///     A function that will retrieve the initial value for the feature instance once its <see cref="Availability" /> has been subscribed.
        /// </param>
        public Feature(Func<T> lazyInitialValue) : this()
        {
            InitializeLazy(lazyInitialValue);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Feature&lt;T&gt;" /> class.
        /// </summary>
        /// <param name="instanceOrFactory">
        ///     An instance for the initial value or a <see cref="Func{T}" /> that will be called to get the initial value once
        ///     <see
        ///         cref="Availability" />
        ///     has been subscribed.
        /// </param>
        internal Feature(object instanceOrFactory) : this()
        {
            var instance = instanceOrFactory as T;

            if (instance != null)
            {
                currentValue.OnNext(instance);
                return;
            }

            var getFeature = instanceOrFactory as Delegate;
            if (getFeature != null)
            {
                InitializeLazy(() => (T) getFeature.DynamicInvoke());
                return;
            }

            throw new ArgumentException(
                string.Format("Parameter 'instanceOrFactory' could not be used. It must be either an instance of {0} or a parameterless delegate that returns an instance of {0}.",
                              typeof (T).FullName));
        }

        private void InitializeLazy(Func<T> lazyInitialValue)
        {
            lazyInitialize = new Lazy<T>(() =>
            {
                T value = lazyInitialValue();
                currentValue.OnNext(value);
                return value;
            });
        }

        /// <summary>
        ///     Produces a non-terminating observable representing the available state over time.
        /// </summary>
        /// <remarks>
        ///     Calls to an instance's methods while it is unavailable can result in an <see cref="InvalidOperationException" />. Observers will receive an
        ///     <see
        ///         cref="IObserver{T}.OnCompleted" />
        ///     call if the state of the feature will never change at runtime. In the event that the instance become faulted, they will receive a
        ///     <see
        ///         cref="IObserver{T}.OnError" />
        ///     call.
        /// </remarks>
        public override IObservable<bool> Availability
        {
            get
            {
                return this
                    .Where(v => v != null)
                    .SelectMany(v =>
                    {
                        // if the feature implements IFeature, use its Availability
                        var feature = v as IFeature;
                        if (feature != null)
                        {
                            return feature.Availability;
                        }

                        // duck typing is supported if the feature implements some interface called IFeature and has an Availability property with the correct return type:
                        var availabilityProperty = v
                            .GetType()
                            .GetProperties()
                            .SingleOrDefault(p =>
                                   p.Name == "Availability" &&
                                   p.ReturnType() == typeof (IObservable<bool>));

                        if (availabilityProperty != null)
                        {
                            return (IObservable<bool>) ((dynamic) v).Availability;
                        }

                        return Observable.Return(true);
                    });
            }
        }

        /// <summary>
        ///     Gets the name for this feature.
        /// </summary>
        public override string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        ///     Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <returns> The observer's interface that enables resources to be disposed. </returns>
        /// <param name="observer"> The object that is to receive notifications. </param>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            var lazyInitialization = lazyInitialize;
            if (lazyInitialization != null)
            {
                try
                {
                    var v = lazyInitialization.Value;
                }
                catch (Exception exception)
                {
                    observer.OnError(exception);
                    return Disposable.Empty;
                }
            }

            return currentValue
                .Where(instance => instance != null)
                .DistinctUntilChanged()
                .Subscribe(observer);
        }

        /// <summary>
        ///     Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public void OnNext(T value)
        {
            currentValue.OnNext(value);
        }

        /// <summary>
        ///     Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            currentValue.OnError(error);
        }

        /// <summary>
        ///     Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
            currentValue.OnCompleted();
        }
    }
}
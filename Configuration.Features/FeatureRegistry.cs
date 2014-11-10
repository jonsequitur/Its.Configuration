using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Its.Configuration.Features
{
    /// <summary>
    /// Allows enabling and configuration of features.
    /// </summary>
    public sealed class FeatureRegistry : IEnumerable<Feature>
    {
        private readonly ConcurrentDictionary<Type, Feature> featuresByType = new ConcurrentDictionary<Type, Feature>();
        private readonly Func<Type, object> featureFactory;

        private static readonly MethodInfo genericAddMethodInfo = typeof (FeatureRegistry)
            .GetMethods()
            .Where(mi => mi.Name == "Add")
            .Single(mi => mi.IsGenericMethod);

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureRegistry"/> class.
        /// </summary>
        /// <param name="featureFactory">A factory delegate used to create feature instances.</param>
        public FeatureRegistry(Func<Type, object> featureFactory = null)
        {
            this.featureFactory = featureFactory ?? (Activator.CreateInstance);
        }

        /// <summary>
        /// Adds the specified feature.
        /// </summary>
        /// <param name="featureType">The feature.</param>
        /// <returns>The same registry instance.</returns>
        public FeatureRegistry Add(Type featureType)
        {
            var add = genericAddMethodInfo.MakeGenericMethod(featureType);

            add.Invoke(this, new object[] { null });

            return this;
        }

        /// <summary>
        /// Adds the specified feature.
        /// </summary>
        /// <param name="createFeature">A function used to instantiate the feature.</param>
        /// <returns>
        /// The same registry instance.
        /// </returns>
        public FeatureRegistry Add<TFeature>(Func<FeatureRegistry, TFeature> createFeature = null)
            where TFeature : class
        {
            Func<TFeature> feature;
            if (createFeature != null)
            {
                feature = (() => createFeature(this));
            }
            else
            {
                feature = (() => (TFeature) featureFactory(typeof (TFeature)));
            }

            featuresByType.AddOrUpdate(typeof (TFeature),
                                       addValueFactory: t =>
                                       {
                                           var f = new Feature<TFeature>(feature);
                                           return f;
                                       },
                                       updateValueFactory: (t, f) =>
                                       {
                                           var f1 = f as Feature<TFeature>;
                                           if (f1 != null)
                                           {
                                               f1.OnNext(feature());
                                           }
                                           return f;
                                       });
            return this;
        }

        /// <summary>
        /// Gets an observable feature of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the feature.</typeparam>
        /// <returns>An observable that will produce a feature instance based on that feature's availability.</returns>
        public IObservable<T> Get<T>() where T : class
        {
            return (IObservable<T>) featuresByType.GetOrAdd(
                typeof (T),
                t => new Feature<T>());
        }
   
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<Feature> GetEnumerator()
        {
            return featuresByType.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
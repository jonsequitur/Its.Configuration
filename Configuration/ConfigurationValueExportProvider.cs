// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Its.Configuration
{
    /// <summary>
    ///   Provides Exports for primitive configuration values.
    /// </summary>
    public class ConfigurationValueExportProvider : ExportProvider
    {
        private readonly GetConfigurationValue getConfigValueString;

        // dictionary to store custom conversions. the key is the target Type, as a string.
        private readonly Dictionary<string, Func<string, object>> typeConverters = Conversion.DefaultTypeConverters.ToDictionary(p => p.Key, p => p.Value);

        private readonly ConcurrentDictionary<string, IEnumerable<Export>> resolvedValues = new ConcurrentDictionary<string, IEnumerable<Export>>();

        /// <summary>
        ///   Initializes a new instance of the <see cref="ConfigurationValueExportProvider" /> class.
        /// </summary>
        /// <param name="configSettings"> The config settings. </param>
        public ConfigurationValueExportProvider(IDictionary<string, object> configSettings)
        {
            if (configSettings == null)
            {
                throw new ArgumentNullException("configSettings");
            }

            getConfigValueString = key =>
            {
                object value;
                configSettings.TryGetValue(key, out value);
                return value;
            };
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="ConfigurationValueExportProvider" /> class.
        /// </summary>
        /// <param name="getConfigValueString"> The get config value string. </param>
        public ConfigurationValueExportProvider(GetConfigurationValue getConfigValueString)
        {
            if (getConfigValueString == null)
            {
                throw new ArgumentNullException("getConfigValueString");
            }
            this.getConfigValueString = getConfigValueString;
        }

        /// <summary>
        ///   Gets all the exports that match the constraint defined by the specified definition.
        /// </summary>
        /// <returns> A collection that contains all the exports that match the specified condition. </returns>
        /// <param name="definition"> The object that defines the conditions of the <see
        ///    cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects to return. </param>
        /// <param name="atomicComposition"> The transactional container for the composition. </param>
        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            return resolvedValues.GetOrAdd(
                definition.Key(),
                k => definition.ResolveExports(getConfigValueString, typeConverters));
        }

        /// <summary>
        /// Specifies a function for performing conversions to type <typeparamref name="TTo" />.
        /// </summary>
        /// <typeparam name="TTo">The type to which values will be converted.</typeparam>
        /// <param name="conversion">The conversion function.</param>
        /// <returns></returns>
        public ConfigurationValueExportProvider RegisterConversion<TTo>(Func<string, TTo> conversion)
        {
            if (conversion == null)
            {
                throw new ArgumentNullException("conversion");
            }

            typeConverters[typeof (TTo).Key()] = value => conversion(value);
            typeConverters[typeof (IObservable<TTo>).Key()] = value => new BehaviorSubject<TTo>(conversion(value));
            return this;
        }

        /// <summary>
        ///   Determines whether a configuration value string can be converted to the specified type.
        /// </summary>
        internal bool SupportsConversionTo(Type targetType)
        {
            return typeConverters.ContainsKey(targetType.Key());
        }

        /// <summary>
        /// Updates a configuration value so that future configuration calls will receive the new value. Instances exposing configurable fields or properties implementing <see cref="IObservable{T}" /> that were set by the current <see cref="ConfigurationValueExportProvider" /> instance will be notified of the change immediately. 
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="value">The new value.</param>
        public ConfigurationValueExportProvider UpdateConfigurationValue(string key, string value)
        {
            var exportsToUpdate = resolvedValues
                .Where(p => p.Key.EndsWith(":" + key, StringComparison.InvariantCulture))
                .Select(resolvedValue => resolvedValue.Value.Single())
                .OfType<UpdateableConfigExport>()
                .ToArray();

            if (exportsToUpdate.Any())
            {
                var changingExportDefinitions = exportsToUpdate.Select(e => e.Definition).ToArray();

                base.OnExportsChanging(new ExportsChangeEventArgs(
                                           changingExportDefinitions,
                                           Enumerable.Empty<ExportDefinition>(),
                                           new AtomicComposition()));

                foreach (var updateable in exportsToUpdate)
                {
                    updateable.Update(value);
                }

                base.OnExportsChanged(new ExportsChangeEventArgs(
                                          changingExportDefinitions,
                                          Enumerable.Empty<ExportDefinition>(),
                                          new AtomicComposition()));
            }

            return this;
        }
    }
}
// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

namespace Its.Configuration
{
    /// <summary>
    /// An abstract class for building catalogs that aggregate parts from multiple assemblies.
    /// </summary>
    public abstract class AssembliesCatalog : ComposablePartCatalog
    {
        private IQueryable<ComposablePartDefinition> parts;
        private readonly List<Exception> errors = new List<Exception>();
        private IEnumerable<Assembly> discoveredAssemblies;

        /// <summary>
        /// Gets the part definitions that are contained in the catalog.
        /// </summary>
        /// <returns>The <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition"/> contained in the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog"/>.</returns>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog"/> object has been disposed of.</exception>
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return parts ?? (parts = GetParts());
            }
        }

        /// <summary>
        /// Gets any errors that occurred during assembling of the catalog.
        /// </summary>
        public IEnumerable<Exception> Errors
        {
            get
            {
                return errors;
            }
        }

        private IQueryable<ComposablePartDefinition> GetParts()
        {
            return Assemblies().Select(a =>
                                       new AssemblyCatalog(a)).AggregateSafely(errors.Add).Parts;
        }

        /// <summary>
        /// Gets all assemblies in the catalog.
        /// </summary>
        public IEnumerable<Assembly> Assemblies()
        {
            return (discoveredAssemblies ?? (discoveredAssemblies = FindAssemblies()));
        }

        /// <summary>
        /// Finds the assemblies to be included in the catalog.
        /// </summary>
        protected abstract IEnumerable<Assembly> FindAssemblies();
    }
}
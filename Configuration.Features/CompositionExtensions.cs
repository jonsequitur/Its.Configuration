// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Its.Recipes;

namespace Its.Configuration.Features
{
    /// <summary>
    ///     Provides methods for composition.
    /// </summary>
    public static class CompositionExtensions
    {
        private static readonly ConcurrentDictionary<string, Type> discoveredTypes = new ConcurrentDictionary<string, Type>();

        /// <summary>
        ///     Calls SatisfyStaticImportsOnce on every static property found in the current AppDomain.
        /// </summary>
        /// <param name="container"> The container. </param>
        public static void SatisfyStaticImportsInAppDomain(this CompositionContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            var types = AppDomain.CurrentDomain
                                 .GetAssemblies()
                                 .SelectMany(GetTypes)
                                 .Where(t => t.StaticImportingMembers().Any());
            foreach (var type in types)
            {
                container.SatisfyStaticImportsOnce(type);
            }
        }

        private static IEnumerable<Type> GetTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // return the types that were successfully loaded
                return ex.Types.Where(t => t != null);
            }
        }

        /// <summary>
        ///     Returns all exported feature types in the catalog.
        /// </summary>
        /// <param name="catalog"> A composable part catalog. </param>
        public static IEnumerable<Type> FeatureTypes(this ComposablePartCatalog catalog)
        {
            return catalog.Parts
                          .SelectMany(p => p.ExportDefinitions)
                          .Where(e => e.Metadata.ContainsKey("ExportTypeIdentity"))
                          .Where(e =>
                                 // Any type is a feature if its name is IFeature, including nested classes (name ending in "+IFeature")
                                 // This allows Its.Configuration to bootstrap features that don't reference Its.Configuration.
                                 ((string) e.Metadata["ExportTypeIdentity"]).EndsWith(".IFeature", StringComparison.OrdinalIgnoreCase) ||
                                 ((string) e.Metadata["ExportTypeIdentity"]).EndsWith("+IFeature", StringComparison.OrdinalIgnoreCase))
                          .Select(exportDefinition => new
                          {
                              exportDefinition,
                              member = exportDefinition.GetType().GetProperty("ExportingLazyMember")
                          })
                          .Select(t =>
                                  (LazyMemberInfo) t.member.GetValue(t.exportDefinition, BindingFlags.GetProperty, null, null, null))
                          .Select(value =>
                                  value.GetAccessors().First() as Type);
        }

        /// <summary>
        ///     Satisfies imports on static members of the specified type.
        /// </summary>
        /// <param name="container"> The composition container. </param>
        /// <param name="type"> The type whose static members should be configured. </param>
        [Obsolete]
        public static void SatisfyStaticImportsOnce(this CompositionContainer container, Type type)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var importMembers = type.StaticImportingMembers();
            var missingExports = new HashSet<string>();

            foreach (var member in importMembers)
            {
                var import = member.GetCustomAttributes(typeof (ImportAttribute), true).Cast<ImportAttribute>().Single();

                var lazyValue = container.GetExports(
                    member.ReturnType(),
                    typeof (object),
                    import.ContractName).FirstOrDefault();

                if (lazyValue == null)
                {
                    if (!import.AllowDefault)
                    {
                        missingExports.Add(String.Format("'{0}' for member {1}.{2}",
                                                         import.ContractName,
                                                         type.FullName,
                                                         member.Name));
                    }
                    continue;
                }

                try
                {
                    var value = lazyValue.Value;
                    member.SetStaticMember(type, value);
                }
                catch (CompositionException ex)
                {
                    throw new CompositionException(
                        string.Format("An error occurred while trying to satisfy static import '{2}' for member {0}.{1}", type.FullName, member.Name, import.ContractName),
                        // leave out the intermediate CompositionException, we're really just making the message more informative
                        ex.InnerException);
                }
            }

            if (missingExports.Any())
            {
                throw new CompositionException(
                    String.Format("Some imports could not be satisfied: {0}{1}",
                                  Environment.NewLine,
                                  String.Join(Environment.NewLine, missingExports)));
            }
        }

        private static void SetStaticMember(this MemberInfo memberInfo, Type type, object value)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(type, value, BindingFlags.Static, null, null, null);
                return;
            }

            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(type, value, BindingFlags.Static, null, null);
            }
        }

        /// <summary>
        ///     Gets a key that is unique for an ImportDefinition instance, incorporating the target type and the contract name.
        /// </summary>
        internal static string Key(this ImportDefinition definition)
        {
            var contractName = definition.ContractName;
            var requiredTypeIdentity = ((ContractBasedImportDefinition) definition).RequiredTypeIdentity;
            return ConfigKey(requiredTypeIdentity, contractName);
        }

        /// <summary>
        ///     Gets the key for a specific combination of the target type (a MEF type identity) and contract name (e.g. the configuration key in the config file).
        /// </summary>
        internal static string ConfigKey(string requiredTypeIdentity, string contractName)
        {
            return String.Format("{0}:{1}", requiredTypeIdentity, contractName);
        }

        internal static string ConfigKey(this Type type, string contractName)
        {
            return String.Format("{0}:{1}", type.Key(), contractName);
        }

        internal static string Key(this Type type)
        {
            return AttributedModelServices.GetContractName(type);
        }

        internal static IEnumerable<MemberInfo> StaticImportingMembers(this Type type)
        {
            return type.GetMembers(
                BindingFlags.Static |
                BindingFlags.GetProperty |
                BindingFlags.GetField |
                BindingFlags.Public |
                BindingFlags.NonPublic)
                       .Where(m => m.GetCustomAttributes(typeof (ImportAttribute), true).Any());
        }

        internal static IEnumerable<MemberInfo> ImportingMembers(this Type type)
        {
            return type.GetMembers(
                BindingFlags.Static |
                BindingFlags.Instance |
                BindingFlags.GetProperty |
                BindingFlags.GetField |
                BindingFlags.Public |
                BindingFlags.NonPublic)
                       .Where(m => m.GetCustomAttributes(typeof (ImportAttribute), true).Any());
        }

        internal static AggregateCatalog AggregateSafely(
            this IEnumerable<AssemblyCatalog> catalogs,
            Action<Exception> onError)
        {
            var catalog = new AggregateCatalog();

            foreach (var assemblyCatalog in catalogs)
            {
                try
                {
                    // trigger possible exceptions due to missing assemblies. if these are going to cause a problem, let them do so on a code path that actually uses them directly, because otherwise it can be very hard to figure out the source of the problem.
                    var parts = assemblyCatalog.Parts;
                    catalog.Catalogs.Add(assemblyCatalog);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    onError(ex);
                }
                catch (FileNotFoundException ex)
                {
                    onError(ex);
                }
                catch (FileLoadException ex)
                {
                    onError(ex);
                }
            }

            return catalog;
        }

        internal static Type ReturnType(this MemberInfo fieldOrProperty)
        {
            Type returnType;
            switch (fieldOrProperty.MemberType)
            {
                case MemberTypes.Property:
                    returnType = ((PropertyInfo) fieldOrProperty).PropertyType;
                    break;
                case MemberTypes.Field:
                    returnType = ((FieldInfo) fieldOrProperty).FieldType;
                    break;
                default:
                    returnType = ((MethodInfo) fieldOrProperty).ReturnType;
                    break;
            }
            return returnType;
        }
    }
}
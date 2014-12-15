// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Its.Configuration.Features
{
    internal static class FeatureActivationInfo
    {
        private static bool featuresActivated;
        private static readonly Func<bool> webStartCompleted = () => false;
        private static readonly Type webStartType;

        static FeatureActivationInfo()
        {
            webStartType = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .Where(a => !a.IsDynamic)
                                    .Where(a => !a.GlobalAssemblyCache)
                                    .SelectMany(a =>
                                    {
                                        try
                                        {
                                            return a.GetTypes();
                                        }
                                        catch (ReflectionTypeLoadException)
                                        {
                                        }
                                        catch (FileNotFoundException)
                                        {
                                        }
                                        return Enumerable.Empty<Type>();
                                    })
                                    .Where(t => !t.IsInterface)
                                    .Where(t => !t.IsGenericTypeDefinition)
                                    .FirstOrDefault(t => t.FullName.Equals("Its.Recipes.WebStart", StringComparison.Ordinal));

            if (webStartType != null)
            {
                var propertyInfo = webStartType.GetProperty("Completed", BindingFlags.Static | BindingFlags.Public);
                if (propertyInfo != null)
                {
                    var webStartCompletedMethod = propertyInfo.GetGetMethod();
                    webStartCompleted = () => (bool) webStartCompletedMethod.Invoke(null, null);
                }
            }
        }

        public static bool FeaturesActivated
        {
            get
            {
                return featuresActivated || webStartCompleted();
            }
            set
            {
                featuresActivated = value;
                if (value)
                {
                    PreventWebStartFromRunning();
                }
            }
        }

        private static void PreventWebStartFromRunning()
        {
            // TODO: (PreventWebStartFromRunning) this doesn't happen early enough to actually prevent WebStart
            if (webStartType != null)
            {
                var propertyInfo = webStartType.GetProperty("Completed", BindingFlags.Static | BindingFlags.Public);
                if (propertyInfo != null)
                {
                    var webStartCompletedMethod = propertyInfo.GetSetMethod();
                    webStartCompletedMethod.Invoke(null, new object[] { true });
                }
            }
        }

        public static IEnumerable<Feature> Features { get; set; }
    }
}
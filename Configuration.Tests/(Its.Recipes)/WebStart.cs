// THIS FILE IS NOT INTENDED TO BE EDITED. 
// 
// This file can be updated in-place using the Package Manager Console. To check for updates, run the following command:
// 
// PM> Get-Package -Updates

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Its.Recipes;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: PreApplicationStartMethod(typeof (WebStart), "StartAll")]

namespace Its.Recipes
{
    /// <summary>
    ///     Allows code to be discovered and called during web application startup.
    /// </summary>
    /// <remarks>
    ///     In order to be called on startup:
    ///     1. Create a non-static class.
    ///     2. Decorate it with with [Export(typeof(IFeature))].
    ///     2.a. IFeature can be defined by you as a private marker interface, or you can reference Its.Configuration.
    ///     3. Add an instance method to your class with the following signature:
    ///     public IObservable&lt;bool&gt; Availability { get { /* ... */ } }
    ///     4. When the observable is subscribed, either:
    ///     * initialize your feature and return true, or
    ///     * don't initialize your feature, and return false.
    ///     The convention used by this class can also be used by Its.Configuration (https://github.com/jonsequitur/Its.Configuration) allowing more complex startup processes, including dependency injection and feature activation dependency chains.
    /// </remarks>
    public static class WebStart
    {
        private static readonly object lockObj = new object();
        private static EventHandler AfterApplicationStart;

        static WebStart()
        {
            Features = Enumerable.Empty<object>();
        }

        public static bool Completed { get; set; }

        public static IEnumerable<object> Features { get; set; }

        public static void DiscoverAndActivateFeatures()
        {
            lock (lockObj)
            {
                if (Completed)
                {
                    return;
                }

                // find all exports of any type named "IFeature"
                var bootstrappedTypes = AppDomain.CurrentDomain
                                                 .GetAssemblies()
                                                 .Where(a => !a.IsDynamic)
                                                 .Where(a => !a.GlobalAssemblyCache)
                                                 .SelectMany(a =>
                                                 {
                                                     try
                                                     {
                                                         return a.GetExportedTypes();
                                                     }
                                                     catch (ReflectionTypeLoadException)
                                                     {
                                                     }
                                                     catch (FileNotFoundException)
                                                     {
                                                     }
                                                     return Enumerable.Empty<Type>();
                                                 })
                                                 .Where(t => t.GetCustomAttributes(typeof (ExportAttribute), false)
                                                              .Cast<ExportAttribute>()
                                                              .Any(e => e.ContractType != null && e.ContractType.Name == "IFeature"));

                Features = bootstrappedTypes.Select(Initialize).ToArray();

                Completed = true;
            }
        }

        public static void StartAll()
        {
            DynamicModuleUtility.RegisterModule(typeof (BootstrapModule));

            AfterApplicationStart += (s, e) => DiscoverAndActivateFeatures();
        }

        private static object Initialize(Type type)
        {
            var instance = Activator.CreateInstance(type);

            var availabilityProperty = type.GetProperty("Availability");
            if (availabilityProperty != null)
            {
                var availability = availabilityProperty.GetValue(instance, null) as IObservable<bool>;
                if (availability != null)
                {
                    availability.Subscribe(new BootstrapObserver());
                }
            }

            return instance;
        }

        private class BootstrapModule : IHttpModule
        {
            public void Dispose()
            {
            }

            public void Init(HttpApplication context)
            {
                AfterApplicationStart(this, EventArgs.Empty);
            }
        }

        private class BootstrapObserver : IObserver<bool>
        {
            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
                throw error;
            }

            public void OnNext(bool value)
            {
            }
        }
    }
}
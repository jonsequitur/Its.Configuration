using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Its.Recipes
{
    internal static class Discover
    {
        public static IEnumerable<Type> DerivedFrom(this IEnumerable<Type> types, Type type)
        {
            return types.Where(type.IsAssignableFrom);
        }

        public static IEnumerable<Type> ConcreteTypes()
        {
            return AppDomain.CurrentDomain
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
                            .Where(t => !t.IsAbstract)
                            .Where(t => !t.IsInterface)
                            .Where(t => !t.IsGenericTypeDefinition);
        }
    }
}
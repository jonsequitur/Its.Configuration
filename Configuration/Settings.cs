// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using Its.Recipes;
using Newtonsoft.Json;

namespace Its.Configuration
{
    /// <summary>
    /// Provides access to settings.
    /// </summary>
    public static class Settings
    {
        private static readonly AzureApplicationSettings azureSettings = new AzureApplicationSettings();
        private static Func<string, SecureString> _certificatePassword = certificateName => null;
        private static readonly ConcurrentDictionary<Type, object> resolvedSettings = new ConcurrentDictionary<Type, object>();
        private static readonly Lazy<IEnumerable<ISettingsSource>> DefaultSources = new Lazy<IEnumerable<ISettingsSource>>(GetDefaultSources);
        private static IEnumerable<ISettingsSource> sources;
        private static Lazy<string[]> precedence;

        private static string[] DefaultPrecedence()
        {
            var values = new string[0];
            var configuredPrecedence = AppSetting("Its.Configuration.Settings.Precedence");

            if (!string.IsNullOrWhiteSpace(configuredPrecedence))
            {
                values = configuredPrecedence.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            }

            return values;
        }

        /// <summary>
        ///     Initializes the <see cref="Settings" /> class.
        /// </summary>
        static Settings()
        {
            Reset();
        }

        /// <summary>
        ///     Creates the source.
        /// </summary>
        /// <param name="getSetting">A delegate that gets a setting based on a provided key.</param>
        /// <param name="name">The name (optional) of the source.</param>
        /// <returns></returns>
        public static ISettingsSource CreateSource(GetSerializedSetting getSetting, string name = null)
        {
            return new AnonymousSettingsSource(getSetting, name);
        }

        /// <summary>
        ///     Deserializes configuration settings.
        /// </summary>
        public static DeserializeSettings Deserialize;

        /// <summary>
        ///     Gets the value for a configuration setting corresponding to the provided key.
        /// </summary>
        public static GetSerializedSetting GetSerializedSetting;

        /// <summary>
        ///     Gets the serialized setting default.
        /// </summary>
        public static string GetSerializedSettingDefault(string key)
        {
            return Sources
                .Select(source => new { source, value = source.GetSerializedSetting(key) })
                .Where(t => !string.IsNullOrWhiteSpace(t.value))
                .Do(t => LogCreation(key, t.source.Name))
                .Select(t => t.value)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the first file in any active .config folder, in order of precedence, that matches the predicate, or null if none match.
        /// </summary>
        /// <param name="matching">A predicate for matching the file.</param>
        /// <returns></returns>
        public static FileInfo GetFile(Func<FileInfo, bool> matching)
        {
            return GetFiles().FirstOrDefault(matching);
        }

        /// <summary>
        /// Gets the  files in the active config folders that match the specified precedence.
        /// </summary>
        public static IEnumerable<FileInfo> GetFiles()
        {
            return Sources.OfType<ConfigDirectorySettings>()
                          .SelectMany(s => s.Files)
                          .Reverse()
                          .Aggregate(new Dictionary<string, FileInfo>(),
                                     (dict, file) =>
                                     {
                                         dict[file.Name.ToLowerInvariant()] = file;
                                         return dict;
                                     })
                          .Values;
        }

        /// <summary>
        ///     Implements the default settings deserialization method, which is to deserialize the specified string using NewtonSoft.Json.
        /// </summary>
        public static object DeserializeDefault(Type targetType, string serialized)
        {
            return JsonConvert.DeserializeObject(serialized, targetType);
        }

        /// <summary>
        ///     Gets a settings object of the specified type.
        /// </summary>
        public static object Get(Type type)
        {
            return resolvedSettings.GetOrAdd(type, t =>
            {
                dynamic settingsFor = Activator.CreateInstance(typeof (For<>).MakeGenericType(type));
                return settingsFor.Value;
            });
        }

        /// <summary>
        ///     Gets a settings object of the specified type.
        /// </summary>
        public static T Get<T>()
        {
            return (T) resolvedSettings.GetOrAdd(typeof (T), t => new For<T>().Value);
        }

        /// <summary>
        ///     Resets settings to the default behavior.
        /// </summary>
        public static void Reset()
        {
            resolvedSettings.Clear();
            SettingsDirectory = Path.Combine(Deployment.Directory, ".config");
            Deserialize = DeserializeDefault;
            GetSerializedSetting = GetSerializedSettingDefault;
            sources = DefaultSources.Value;
            precedence = new Lazy<string[]>(DefaultPrecedence);
        }

        /// <summary>
        ///     Gets or sets the sources that are used to look up settings.
        /// </summary>
        /// <remarks>Each source is called in order until one returns a non-null, non-whitespace value, which is the value that is used. Setting this property to null resets it to the default behavior.</remarks>
        public static IEnumerable<ISettingsSource> Sources
        {
            get
            {
                return sources ?? DefaultSources.Value;
            }
            set
            {
                sources = value;
            }
        }

        /// <summary>
        ///     Gets or sets the precedence of settings folders.
        /// </summary>
        public static string[] Precedence
        {
            get
            {
                return precedence.Value;
            }
            set
            {
                precedence = new Lazy<string[]>(() => value ?? new string[0]);
            }
        }

        /// <summary>
        ///     Gets or sets the root directory where file-based settings are looked up.
        /// </summary>
        public static string SettingsDirectory { get; set; }

        /// <summary>
        /// Gets or sets a function to access the password for a given certificate, given a string representing the certificate's file name.
        /// </summary>
        public static Func<string, SecureString> CertificatePassword
        {
            get
            {
                return _certificatePassword ?? (s => null);
            }
            set
            {
                _certificatePassword = value;
            }
        }

        /// <summary>
        ///     Gets a setting from AppSettings, checking Azure configuration first and falling back to web.config/app.config if the setting is not found or if it is empty.
        /// </summary>
        /// <param name="key">The key for the setting.</param>
        /// <returns>The configured value.</returns>
        public static string AppSetting(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (azureSettings.IsAvailable)
            {
                value = azureSettings.GetSerializedSetting(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return ConfigurationManager.AppSettings[key];
        }

        private static IEnumerable<ISettingsSource> GetDefaultSources()
        {
            yield return new EnvironmentVariableSettingsSource();

            if (azureSettings.IsAvailable)
            {
                // Azure cscfg
                yield return azureSettings;
            }

            foreach (var value in Precedence)
            {
                // e.g. \bin\.config\{value}
                yield return new ConfigDirectorySettings(Path.Combine(SettingsDirectory, value));
            }

            // e.g. \bin\.config
            yield return new ConfigDirectorySettings(SettingsDirectory);

            yield return new AppConfigSettingsSource();
        }

        internal static void LogCreation(string key, string source)
        {
            Trace.WriteLine(string.Format("Resolved setting '{0}' from {1}", key, source), "Its.Configuration");
        }

        /// <summary>
        ///     Provides access to settings for a specified type.
        /// </summary>
        /// <typeparam name="T">The type that holds the configuration settings.</typeparam>
        public class For<T>
        {
            private static string key = BuildKey();

            /// <summary>
            ///     Initializes a new instance of the <see cref="For{T}" /> class.
            /// </summary>
            public For()
            {
                var configSetting = GetSerializedSetting(Key);

                var targetType = typeof (T);

                if (typeof (T).IsAbstract)
                {
                    targetType = Discover.ConcreteTypes().Single(t => string.Equals(t.Name, Key, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(configSetting))
                {
                    Value = (T) Deserialize(targetType, configSetting);
                }
                else
                {
                    try
                    {
                        Value = (T) Activator.CreateInstance(targetType);
                        LogCreation(BuildKey(), " new instance. No configuration found.");
                    }
                    catch (MissingMethodException)
                    {
                        if (typeof (T).IsAbstract)
                        {
                            throw new ConfigurationErrorsException(
                                string.Format(
                                    "Settings.For is unable to create an instance of {0} because it is abstract. You should either change the class definition to be concrete or redirect settings resolution to another class by adding an appSetting entry, for example: \n    <add key=\"{1}\" value=\"{{NAME_OF_CONCRETE_TYPE}}\" />",
                                    typeof (T).AssemblyQualifiedName,
                                    typeof (T).Name));
                        }

                        throw new ConfigurationErrorsException(
                            string.Format(
                                "Type {0} cannot be instantiated without some additional setup because it does not have a parameterless constructor. You can fix this by setting Settings<{1}>.Deserialize with a delegate that can instantiate this type.",
                                typeof (T).AssemblyQualifiedName,
                                typeof (T).Name));
                    }
                }
            }

            /// <summary>
            ///     Gets the configured settings for type <typeparamref name="T" />.
            /// </summary>
            public T Value { get; private set; }

            /// <summary>
            ///     Specifies how the settings for type <typeparamref name="T" /> should be deserialized.
            /// </summary>
            public static DeserializeSettings Deserialize = Settings.Deserialize;

            /// <summary>
            ///     Specifies how the settings for type <typeparamref name="T" /> should be retrieved.
            /// </summary>
            public static GetSerializedSetting GetSerializedSetting = Settings.GetSerializedSetting;

            /// <summary>
            ///     Gets or sets the key used to look up the settings in configuration.
            /// </summary>
            /// <exception cref="System.ArgumentException">The key cannot be null, empty, or consist entirely of whitespace.</exception>
            public static string Key
            {
                get
                {
                    return key;
                }
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentException("The key cannot be null, empty, or consist entirely of whitespace.");
                    }
                    key = value;
                }
            }

            private static string BuildKey()
            {
                var defaultKey = FlattenGenericNames(typeof (T)).ToDelimitedString("");

                // abstract members indicate a redirect to a concrete type, specified in AppSettings.
                if (typeof (T).IsAbstract)
                {
                    var redirectedKey = AppSetting(defaultKey);
                    if (!string.IsNullOrWhiteSpace(redirectedKey))
                    {
                        return redirectedKey;
                    }
                }

                return defaultKey;
            }

            private static IEnumerable<string> FlattenGenericNames(Type type)
            {
                if (!type.IsGenericType)
                {
                    yield return type.Name;
                }
                else
                {
                    var genericName = type.GetGenericTypeDefinition().Name;
                    genericName = genericName.Substring(0, genericName.IndexOf("`", StringComparison.InvariantCulture));
                    yield return genericName;

                    yield return "(";

                    bool first = true;

                    foreach (var genericTypeArgument in type.GetGenericArguments())
                    {
                        if (!first)
                        {
                            yield return ",";
                        }
                        yield return FlattenGenericNames(genericTypeArgument).ToDelimitedString("");
                        first = false;
                    }

                    yield return ")";
                }
            }
        }

    }
}
// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Its.Configuration
{
    /// <summary>
    /// Provides settings based on a set of files in a specified configuration folder.
    /// </summary>
    public class ConfigDirectorySettings : ISettingsSource
    {
        private readonly string directoryPath;
        private readonly Dictionary<string, string> fileContents = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> secureSettings = new HashSet<string>();
        private readonly List<FileInfo> files = new List<FileInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigDirectorySettings"/> class.
        /// </summary>
        /// <param name="directoryPath">The directory path to where the configuration files are located.</param>
        public ConfigDirectorySettings(string directoryPath)
        {
            this.directoryPath = directoryPath;
            ReadFiles();
        }

        private void ReadFiles()
        {
            var directory = new DirectoryInfo(directoryPath);

            if (directory.Exists)
            {
                foreach (var file in directory.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
                                              .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
                {
                    files.Add(file);
                    if (string.Equals(file.Extension, ".json", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var stream = file.OpenRead())
                        using (var reader = new StreamReader(stream))
                        {
                            var content = reader.ReadToEnd();
                            var key = GetKey(file);

                            fileContents.Add(key, content);
                        }
                    }
                    else if (string.Equals(file.Extension, ".secure", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var stream = file.OpenRead())
                        using (var reader = new StreamReader(stream))
                        {
                            var content = reader.ReadToEnd();
                            var key = GetKey(file);

                            key = key.Remove(key.Length - ".json".Length);
                            secureSettings.Add(key);

                            try
                            {
                                fileContents.Add(key, content);
                            }
                            catch (ArgumentException)
                            {
                                throw new ConfigurationErrorsException(string.Format("Found conflicting settings file: {0}", file.FullName));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets a settings string corresponding to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A string representing serialized settings.</returns>
        public string GetSerializedSetting(string key)
        {
            string value;

            if (fileContents.TryGetValue(key, out value))
            {
                if (secureSettings.Contains(key))
                {
                    var certs = Settings.GetCertificatesFromConfigDirectory();

                    return value.Decrypt(certs.ToArray());
                }

                return value;
            }

            return null;
        }

        /// <summary>
        /// Gets the list of files found in the configuration directory.
        /// </summary>
        public List<FileInfo> Files
        {
            get
            {
                return files;
            }
        }

        /// <summary>
        ///     Gets the name of the settings source.
        /// </summary>
        public string Name
        {
            get
            {
                return "settings folder (" + directoryPath + ")";
            }
        }

        private static string GetKey(FileInfo file)
        {
            return file.Name.Remove(file.Name.Length - file.Extension.Length);
        }
    }
}
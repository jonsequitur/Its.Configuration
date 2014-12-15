// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Its.Configuration
{
    /// <summary>
    ///     Provides information about an application's physical deployment.
    /// </summary>
    public static class Deployment
    {
        private static readonly string directory = GetDeploymentDirectory();

        private static string GetDeploymentDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// Gets the path of the directory where the application is running.
        /// </summary>
        public static string Directory
        {
            get
            {
                return directory;
            }
        }
    }
}
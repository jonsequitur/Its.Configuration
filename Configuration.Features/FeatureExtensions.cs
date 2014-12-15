// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Reactive.Linq;

namespace Its.Configuration.Features
{
    /// <summary>
    /// Extends the functionality of features.
    /// </summary>
    public static class FeatureExtensions
    {
        /// <summary>
        /// Gets a value indicating whether this feature is available at the moment.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is available; otherwise, <c>false</c>.
        /// </value>
        public static bool IsAvailable(this IFeature feature)
        {
            return feature.Availability.MostRecent(false).FirstOrDefault();
        }
    }
}
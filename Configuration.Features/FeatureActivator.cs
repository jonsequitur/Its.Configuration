using System;
using System.Linq;

namespace Its.Configuration.Features
{
    /// <summary>
    /// Manages activate and deactivate calls based on observers and the availability of dependencies.
    /// </summary>
    public class FeatureActivator : FeatureActivator<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureActivator"/> class.
        /// </summary>
        /// <param name="activate">A function to be called when the first observer subscribes and all dependencies (if any) are available.</param>
        /// <param name="deactivate">A function to be called when any dependency becomes unavailable.</param>
        /// <param name="dependsOn">A set of dependencies, all of which must produce a true result in order for activation to be triggered.</param>
        public FeatureActivator(
            Action activate,
            Action deactivate = null,
            params IObservable<bool>[] dependsOn) :
                base(
                activate: () =>
                {
                    activate();
                    return true;
                },
                deactivate: () =>
                {
                    if (deactivate != null)
                    {
                        deactivate();
                    }
                    return false;
                },
                dependsOn: dependsOn)
        {
        }
    }
}
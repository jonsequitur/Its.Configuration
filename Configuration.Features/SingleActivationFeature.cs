using System;
using System.Reactive.Linq;

namespace Its.Configuration.Features
{
    /// <summary>
    ///     A feature that is activated once and cannot afterward be deactivated.
    /// </summary>
    public abstract class SingleActivationFeature : IFeature
    {
        private readonly FeatureActivator activator;

        protected SingleActivationFeature()
        {
            activator = new FeatureActivator(Activate,
                                             dependsOn: Observable.Return(IsAvailable));
        }

        protected virtual bool IsAvailable
        {
            get
            {
                return true;
            }
        }

        protected abstract void Activate();

        public IObservable<bool> Availability
        {
            get
            {
                return activator;
            }
        }
    }
}
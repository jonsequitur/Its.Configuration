using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;

namespace Its.Configuration.Tests.Recipes
{
    [Export("Feature", typeof (ISupportInitialize))]
    public class DependentFeature : ISupportInitialize
    {
        public static int CtorCount = 0;
        public static int BeginInitCount = 0;
        public static int EndInitCount = 0;

        public static Action<PrimaryFeature> OnCtor = feature => { };

        public DependentFeature(PrimaryFeature primaryFeature)
        {
            Interlocked.Increment(ref CtorCount);
            OnCtor(primaryFeature);
        }

        public void BeginInit()
        {
            Interlocked.Increment(ref BeginInitCount);
        }

        public void EndInit()
        {
            Interlocked.Increment(ref EndInitCount);
        }
    }
}
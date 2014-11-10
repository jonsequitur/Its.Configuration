// THIS FILE IS NOT INTENDED TO BE EDITED. 
// 
// It has been imported using NuGet from the Its.Recipes project (http://codebox/ItsRecipes). 
// 
// This file can be updated in-place using the Package Manager Console. To check for updates, run the following command:
// 
// PM> Get-Package -Updates

using System;
using System.Linq;

namespace Its.Recipes
{
    /// <summary>
    /// A disposable that calls a specified action when disposed.
    /// </summary>
#if !RecipesProject
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
    internal class AnonymousDisposable : IDisposable
    {
        private readonly Action dispose;
        private readonly object lockObject = new object();
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousDisposable" /> class.
        /// </summary>
        /// <param name="dispose">The action to be called when the anonymous disposable is disposed.</param>
        /// <exception cref="ArgumentNullException">dispose</exception>
        public AnonymousDisposable(Action dispose)
        {
            if (dispose == null)
            {
                throw new ArgumentNullException("dispose");
            }
            this.dispose = dispose;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            lock (lockObject)
            {
                if (disposed)
                {
                    return;
                }
                disposed = true;
                dispose();
            }
        }
    }
}
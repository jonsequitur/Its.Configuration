using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Its.Configuration
{
    /// <summary>
    ///     A catalog for all of the types found in all assemblies in the application's physical deployment location.
    /// </summary>
    public class DeploymentCatalog : AppDomainCatalog
    {
        protected override IEnumerable<Assembly> FindAssemblies()
        {
            var files = Directory.EnumerateFiles(Deployment.Directory, "*.dll");

            var physicalDeploymentAssemblies = files.Select(LoadAssembly).Where(a => a != null);

            return base.FindAssemblies().Concat(physicalDeploymentAssemblies).Distinct();
        }

        private static Assembly LoadAssembly(string file)
        {
            try
            {
                return Assembly.Load(AssemblyName.GetAssemblyName(file));
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
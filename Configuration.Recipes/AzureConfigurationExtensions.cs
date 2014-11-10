using System.Linq;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Its.Configuration
{
    internal static class AzureConfigurationExtensions
    {
        public static ConfigurationValueExportProvider UpdateWhenRoleEnvironmentChanges(this ConfigurationValueExportProvider exportProvider)
        {
            RoleEnvironment.Changed += (sender, args) =>
            {
                foreach (var change in args.Changes.OfType<RoleEnvironmentConfigurationSettingChange>())
                {
                    exportProvider.UpdateConfigurationValue(
                        change.ConfigurationSettingName,
                        RoleEnvironment.GetConfigurationSettingValue(change.ConfigurationSettingName));
                }
            };

            return exportProvider;
        }
    }
}
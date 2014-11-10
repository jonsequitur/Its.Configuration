namespace Its.Configuration
{
    /// <summary>
    /// Gets a raw value from configuration.
    /// </summary>
    /// <param name="key">The key for the configuration value.</param>
    public delegate object GetConfigurationValue(string key);
}
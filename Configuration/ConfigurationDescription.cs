using System;

namespace Its.Configuration
{
    internal class ConfigurationDescription
    {
        // TODO: (ConfigurationDescription) finish this
        public string Id { get; set; }
        public string Name { get; set; }
        public Type DeclaringType { get; set; }
        public Type ExpectedType { get; set; }
        public bool AllowDefault { get; set; }
    }
}
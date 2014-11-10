using System;
using System.Linq;

namespace Its.Configuration.Tests
{
    public partial class EnvironmentSettings
    {
        public string Name { get; set; }
        public bool IsLocal { get; set; }
        public bool IsTest { get; set; }
    }
}
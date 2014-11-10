using System;
using System.ComponentModel.Composition;

namespace Its.Configuration.Tests
{
    [Export]
    public static class SomethingConfigurableAndStaticHavingSpecialTypes
    {
        [Import("some_uri", AllowDefault = true)]
        public static Uri SomeUri { get; set; }
    }
}
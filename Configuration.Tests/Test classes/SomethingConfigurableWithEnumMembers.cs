using System.ComponentModel.Composition;
using System.IO;

namespace Its.Configuration.Tests
{
    [Export]
    public class SomethingConfigurableWithEnumMembers
    {
        [Import("file-attributes")]
        public FileAttributes FileAttributes;

        [Import("file-access")]
        public FileAccess? FileAccess;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Its.Configuration.Tests
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        public virtual void Run_when_source_is_null_does_not_throw()
        {
            IEnumerable<string> nullEnumerable = null;

            Assert.Throws<ArgumentNullException>(() => nullEnumerable.Run());
        }

        [Test]
        public virtual void ForEach_when_source_is_null_throws()
        {
            IEnumerable<string> nullEnumerable = null;

            Assert.Throws<ArgumentNullException>(() => nullEnumerable.ForEach(s => { }));
        }

        [Test]
        public void When_source_sequence_is_null_OrEmpty_returns_empty_sequence()
        {
            IEnumerable<string> nullSequence = null;

            Assert.That(nullSequence.OrEmpty().SequenceEqual(new string[] { }));
        }

        [Test]
        public void When_source_sequence_has_items_OrEmpty_returns_same_sequence()
        {
            IEnumerable<string> sequence = new[] { "this", "that" };

            Assert.AreSame(sequence, sequence.OrEmpty());
        }

        [Test]
        public void ToDelimitedString_produces_the_same_output_as_string_Join()
        {
            var seq = Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray();

            Assert.That(seq.ToDelimitedString("|"), Is.EqualTo(string.Join("|", seq)));
        }
    }
}
using Expressium.LivingDoc.Parsers;
using Io.Cucumber.Messages.Types;
using System;

namespace Expressium.LivingDoc.UnitTests.Parsers
{
    internal class MessagesUtilitiesTests
    {
        [Test]
        public void TimestampConversion_PreservesUtcKind()
        {
            var timestamp = new Timestamp(0, 0);

            var result = timestamp.ToDateTime();

            Assert.That(result, Is.EqualTo(DateTime.UnixEpoch));
            Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void TimestampDuration_IsCalculatedFromUtcInstants()
        {
            var start = new Timestamp(0, 0);
            var end = new Timestamp(2, 500000000);

            var result = start.ToTimeSpan(end);

            Assert.That(result, Is.EqualTo(TimeSpan.FromSeconds(2.5)));
        }

        [Test]
        public void CapitalizeWords_CapitalizesEachWord()
        {
            var input = "hello world";
            var result = input.CapitalizeWords();
            Assert.That(result, Is.EqualTo("Hello World"));
        }

        [Test]
        public void CapitalizeWords_HandlesEmptyAndNull()
        {
            Assert.That(((string)null).CapitalizeWords(), Is.Null);
            Assert.That(string.Empty.CapitalizeWords(), Is.Empty);
        }

        [Test]
        public void CapitalizeWords_HandlesSingleWord()
        {
            Assert.That("test".CapitalizeWords(), Is.EqualTo("Test"));
        }
    }
}
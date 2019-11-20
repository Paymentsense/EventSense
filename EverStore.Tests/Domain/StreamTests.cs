using System;
using EverStore.Domain;
using Xunit;

namespace EverStore.Tests.Domain
{
    public class StreamTests
    {
        [Fact]
        public void StreamIsWellFormed()
        {
            var stream = "contact_1234";

            Stream.Parse(stream, out string streamAggregate, out string streamId);

            Assert.Equal("contact", streamAggregate);
            Assert.Equal("1234", streamId);
        }

        [Theory]
        [InlineData("contact1234")]
        [InlineData("contact_")]
        [InlineData("_1234")]
        [InlineData("contact")]
        [InlineData("")]
        [InlineData(" ")]
        public void StreamIsBadlyFormed(string actualStream)
        {
            Assert.Throws<ArgumentException>(() => Stream.Parse(actualStream, out string streamAggregate, out string streamId));
        }
    }
}

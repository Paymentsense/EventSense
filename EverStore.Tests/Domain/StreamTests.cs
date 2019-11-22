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
        [InlineData("contact_783daa7e-1de6-45d0-874e-b502eab0d6ab")]
        public void StreamIsBadlyFormed(string actualStream)
        {
            Assert.Throws<ArgumentException>(() => Stream.Parse(actualStream, out string streamAggregate, out string streamId));
        }
    }
}

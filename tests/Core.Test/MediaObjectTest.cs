namespace EagleEye.Core.Test
{
    using System;

    using FluentAssertions;

    using Xunit;

    public class MediaObjectTest
    {
        [Fact]
        public void CtorWithNullFilename_ThrowsArgumentNullExceptionTest()
        {
            // arrange

            // act
            Action act = () => _ = new MediaObject(null);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CtorWithInvalidFilename_ThrowsArgumentExceptionTest()
        {
            // arrange
            var filename = " ";

            // act
            Action act = () => _ = new MediaObject(filename);

            // assert
            act.Should().Throw<ArgumentException>();
        }
    }
}

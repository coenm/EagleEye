namespace EagleEye.Core.Test
{
    using System;

    using FluentAssertions;

    using Xunit;

    public class MediaObjectTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public void CtorWithInvalidFilename_ThrowsExceptionTest(string filename)
        {
            // arrange

            // act
            Action act = () => _ = new MediaObject(filename);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
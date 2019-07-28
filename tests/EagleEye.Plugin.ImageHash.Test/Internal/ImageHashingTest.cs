namespace EagleEye.ImageHash.Test.Internal
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    using FluentAssertions;
    using Xunit;

    using Sut = EagleEye.ImageHash.Internal.ImageHashing;

    public class ImageHashingTest
    {
        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Test")]
        public void Calculate_ShouldThrow_WhenInputStreamIsNull()
        {
            // arrange
            Stream inputStream = null;

            // act
            Action act = () => _ = Sut.Calculate(inputStream);

            // assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Calculate_ShouldThrow_WhenInputStreamIsNotAnImage()
        {
            // arrange
            Stream inputStream = new MemoryStream(new byte[15]);

            // act
            Action act = () => _ = Sut.Calculate(inputStream);

            // assert
            act.Should().Throw<InvalidDataException>();
        }
    }
}

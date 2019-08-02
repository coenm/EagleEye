namespace EagleEye.Photo.Domain.Test.CommandHandlers.Exceptions
{
    using EagleEye.Photo.Domain.CommandHandlers.Exceptions;
    using FluentAssertions;
    using Xunit;

    public class PhotoAlreadyExistsExceptionTest
    {
        [Fact]
        public void Filename_ShouldBeSet_WhenCreatingException()
        {
            // arrange
            const string filename = "dummy";

            // act
            var sut = new PhotoAlreadyExistsException(filename);

            // assert
            sut.Filename.Should().Be(filename);
        }

        [Fact]
        public void Message_ShouldBeSet_WhenCreatingException()
        {
            // arrange
            const string filename = "dummy";

            // act
            var sut = new PhotoAlreadyExistsException(filename);

            // assert
            sut.Message.Should().Be("Photo with filename 'dummy' already exists.");
        }
    }
}

namespace EagleEye.Photo.Domain.Test
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.Domain.Services;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class ModuleInitializerTest
    {
        private readonly IMediaFilenameRepository mediaRepository;
        private readonly IEventExporter eventExporter;
        private readonly ModuleInitializer sut;

        public ModuleInitializerTest()
        {
            mediaRepository = A.Fake<IMediaFilenameRepository>();
            eventExporter = A.Fake<IEventExporter>();
            sut = new ModuleInitializer(mediaRepository, eventExporter);
        }

        [Fact]
        public async Task InitializeAsync_ShouldGetAllEventsFromEventExporter()
        {
            // arrange

            // act
            await sut.InitializeAsync();

            // assert
            A.CallTo(() => eventExporter.GetAsync(DateTime.MinValue, CancellationToken.None)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task InitializeAsync_AddFileNamesToRepository_WhenEventsContainPhotoCreatedEvent()
        {
            // arrange
            A.CallTo(() => eventExporter.GetAsync(A<DateTime>._, A<CancellationToken>._))
             .Returns(new IEvent[]
                      {
                          new PhotoCreated(Guid.NewGuid(), "test.jpg", "dummy", new byte[32]),
                          new FileHashUpdated(Guid.NewGuid(), new byte[32]),
                          new PhotoCreated(Guid.NewGuid(), "test2.jpg", "dummy", new byte[32]),
                          new PhotoCreated(Guid.NewGuid(), "test3.jpg", "dummy", new byte[32]),
                      });

            // act
            await sut.InitializeAsync();

            // assert
            A.CallTo(() => eventExporter.GetAsync(DateTime.MinValue, CancellationToken.None)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => mediaRepository.Add("test.jpg")).MustHaveHappenedOnceExactly())
             .Then(A.CallTo(() => mediaRepository.Add("test2.jpg")).MustHaveHappenedOnceExactly())
             .Then(A.CallTo(() => mediaRepository.Add("test3.jpg")).MustHaveHappenedOnceExactly());
            A.CallTo(() => mediaRepository.Add(A<string>._)).MustHaveHappened(3, Times.Exactly);
        }

        [Fact]
        public void InitializeAsync_ShouldThrow_WhenEventExportThrows()
        {
            // arrange
            A.CallTo(() => eventExporter.GetAsync(A<DateTime>._, A<CancellationToken>._)).Throws(new Exception("thrown by test"));

            // act
            Func<Task> act = async () => await sut.InitializeAsync();

            // assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void InitializeAsync_ShouldThrow_WhenEventRepositoryThrows()
        {
            // arrange
            A.CallTo(() => eventExporter.GetAsync(A<DateTime>._, A<CancellationToken>._))
             .Returns(new IEvent[]
                      {
                          new PhotoCreated(Guid.NewGuid(), "test.jpg", "dummy", new byte[32]),
                          new FileHashUpdated(Guid.NewGuid(), new byte[32]),
                          new PhotoCreated(Guid.NewGuid(), "test2.jpg", "dummy", new byte[32]),
                          new PhotoCreated(Guid.NewGuid(), "test3.jpg", "dummy", new byte[32]),
                      });
            A.CallTo(() => mediaRepository.Add("test3.jpg")).Throws(new Exception("thrown by test"));

            // act
            Func<Task> act = async () => await sut.InitializeAsync();

            // assert
            act.Should().Throw<Exception>();
        }
    }
}

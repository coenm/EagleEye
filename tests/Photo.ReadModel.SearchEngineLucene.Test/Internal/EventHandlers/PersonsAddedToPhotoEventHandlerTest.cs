﻿namespace Photo.ReadModel.SearchEngineLucene.Test.Internal.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class PersonsAddedToPhotoEventHandlerTest
    {
        private readonly PersonsAddedToPhotoEventHandler sut;
        private readonly IPhotoIndex photoIndex;

        public PersonsAddedToPhotoEventHandlerTest()
        {
            photoIndex = A.Fake<IPhotoIndex>();
            sut = new PersonsAddedToPhotoEventHandler(photoIndex);
        }

        [Fact]
        public async Task Handle_ShouldSearchForPhoto()
        {
            // arrange
            var guid = Guid.NewGuid();

            // act
            await sut.Handle(new PersonsAddedToPhoto(guid));

            // assert
            A.CallTo(() => photoIndex.Search(guid)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenPhotoDoesNotExist()
        {
            // arrange
            var guid = Guid.NewGuid();
            A.CallTo(() => photoIndex.Search(guid)).Returns(null);

            // act
            await sut.Handle(new PersonsAddedToPhoto(guid, "Zoo"));

            // assert
            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<Photo>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Handle_ShouldReIndexPhotoWithUpdatedPersons_WhenPhotoExists()
        {
            // arrange
            var guid = Guid.NewGuid();
            Photo newPhoto = null;
            var photoSearchResult = new PhotoSearchResult(1)
            {
                Persons = new List<string>
                {
                    "Holiday",
                },
            };

            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<Photo>._))
                .Invokes(call => { newPhoto = call.Arguments[0] as Photo; });
            A.CallTo(() => photoIndex.Search(guid)).Returns(photoSearchResult);

            // act
            await sut.Handle(new PersonsAddedToPhoto(guid, "Zoo"));

            // assert
            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<Photo>._)).MustHaveHappenedOnceExactly();
            newPhoto.Should().NotBeNull();
            newPhoto.Persons.Should().BeEquivalentTo("Holiday", "Zoo");
        }

        [Fact]
        public async Task Handle_ShouldNotReIndexPhoto_WhenPhotoAlreadyContainedUpdatedPersons()
        {
            // arrange
            var guid = Guid.NewGuid();
            var photoSearchResult = new PhotoSearchResult(1)
            {
                Persons = new List<string>
                {
                    "Holiday",
                    "Zoo",
                },
            };

            A.CallTo(() => photoIndex.Search(guid)).Returns(photoSearchResult);

            // act
            await sut.Handle(new PersonsAddedToPhoto(guid, "Zoo"));

            // assert
            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<Photo>._)).MustNotHaveHappened();
        }
    }
}

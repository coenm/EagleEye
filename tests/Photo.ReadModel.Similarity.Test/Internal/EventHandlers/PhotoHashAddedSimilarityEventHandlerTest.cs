namespace Photo.ReadModel.Similarity.Test.Internal.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EventHandlers;
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs;
    using FakeItEasy;
    using FluentAssertions;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using Photo.ReadModel.Similarity.Test.Mocks;
    using Xunit;

    public class PhotoHashAddedSimilarityEventHandlerTest : IDisposable
    {
        private const int Version = 0;
        private const string HashAlgorithm1 = "hashAlgo1";
        private const string HashAlgorithm2 = "hashAlgo2";

        private readonly InMemorySimilarityDbContextFactory contextFactory;
        private readonly IInternalStatelessSimilarityRepository repository;
        private readonly IBackgroundJobClient hangFireClient;
        private readonly PhotoHashAddedSimilarityEventHandler sut;
        private readonly List<Job> jobsAdded;
        private readonly DateTimeOffset timestamp;

        public PhotoHashAddedSimilarityEventHandlerTest()
        {
            timestamp = DateTimeOffset.UtcNow;

            contextFactory = new InMemorySimilarityDbContextFactory();
            contextFactory.Initialize().GetAwaiter().GetResult();

            repository = A.Fake<InternalSimilarityRepository>();

            hangFireClient = A.Fake<IBackgroundJobClient>();

            jobsAdded = new List<Job>();
            A.CallTo(() => hangFireClient.Create(A<Job>._, A<IState>._))
                .Invokes(call => jobsAdded.Add(call.Arguments[0] as Job));

            sut = new PhotoHashAddedSimilarityEventHandler(repository, contextFactory, hangFireClient);
        }

        public void Dispose() => contextFactory.Dispose();
    }
}

namespace Photo.ReadModel.Similarity.Test.Internal.EventHandlers
{
    using System;
    using System.Collections.Generic;

    using FakeItEasy;
    using FluentAssertions;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;

    internal class HangFireTestHelper
    {
        private readonly List<Job> jobsAdded;

        public HangFireTestHelper()
        {
            HangFireClient = A.Fake<IBackgroundJobClient>();

            jobsAdded = new List<Job>();
            A.CallTo(() => HangFireClient.Create(A<Job>._, A<IState>._))
             .Invokes(call => jobsAdded.Add(call.Arguments[0] as Job));
        }

        public IBackgroundJobClient HangFireClient { get; }

        public void AssertSingleHangFireJobHasBeenCreated(Type type, string methodName, params object[] parameters)
        {
            A.CallTo(() => HangFireClient.Create(A<Job>._, A<IState>._)).MustHaveHappenedOnceExactly();
            jobsAdded.Should().HaveCount(1);
            jobsAdded.Should()
                     .Contain(item =>
                                  item.Type == type
                                  &&
                                  item.Method.Name == methodName)
                     .Which.Args.Should().BeEquivalentTo(parameters);
        }

        public void AssertNoJobHasBeenCreated()
        {
            A.CallTo(() => HangFireClient.Create(A<Job>._, A<IState>._)).MustNotHaveHappened();
        }
    }
}

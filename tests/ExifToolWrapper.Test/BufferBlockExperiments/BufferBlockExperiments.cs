namespace EagleEye.ExifToolWrapper.Test.BufferBlockExperiments
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    using FluentAssertions;

    using Xunit;
    using Xunit.Categories;

    [Exploratory]
    public class BufferBlockExperiments
    {
        private readonly CancellationTokenSource ctsQueue;
        private readonly BufferBlock<int> queue;
        private readonly List<CancellationTokenSource> ctsSources;

        public BufferBlockExperiments()
        {
            ctsQueue = new CancellationTokenSource();
            queue = new BufferBlock<int>(new DataflowBlockOptions
            {
                BoundedCapacity = 1,
                CancellationToken = ctsQueue.Token,
                EnsureOrdered = true,
                TaskScheduler = TaskScheduler.Current,
            });

            ctsSources = new List<CancellationTokenSource>();
        }

        [Fact]
        public async Task CancelQueueWillResultInFinishedTasksWithoutProcessingThemTest()
        {
            // arrange
            InitCancellationTokens(3);
            var task1 = queue.SendAsync(1, ctsSources[0].Token);
            var task2 = queue.SendAsync(2, ctsSources[1].Token);
            var task3 = queue.SendAsync(3, ctsSources[2].Token);

            // act
            ctsQueue.Cancel();

            // assert
            // the first was already in the queue and is therefore true?!
            await AssertTaskReturnsAsync(task1, true).ConfigureAwait(false);

            // the other two were postponed and therefore return false!?
            await AssertTaskReturnsAsync(task2, false).ConfigureAwait(false);
            await AssertTaskReturnsAsync(task3, false).ConfigureAwait(false);
        }

        [Fact]
        public async Task BufferBlockWithBoundedCapacityOfOneShouldRespectOrderTest()
        {
            // arrange
            const int indexToCancel = 55;
            const int size = 1000;
            InitCancellationTokens(size);

            // act
            for (var index = 0; index < size; index++)
            {
                _ = queue.SendAsync(index, ctsSources[index].Token);
            }

            ctsSources[indexToCancel].Cancel();

            // assert
            for (var index = 0; index < indexToCancel; index++)
            {
                var number = await queue.ReceiveAsync(CancellationToken.None).ConfigureAwait(false);
                number.Should().Be(index);
            }

            for (var index = indexToCancel + 1; index < size; index++)
            {
                var number = await queue.ReceiveAsync(CancellationToken.None).ConfigureAwait(false);
                number.Should().Be(index);
            }
        }

        [Fact]
        public async Task SendItemThatIsCancelledBeforeAddedToQueueWillResultInCancelledTaskTest()
        {
            // arrange
            InitCancellationTokens(3);

            // act
            var task1 = queue.SendAsync(1, ctsSources[0].Token);
            var task2 = queue.SendAsync(2, ctsSources[1].Token);
            var task3 = queue.SendAsync(3, ctsSources[2].Token);

            ctsSources[1].Cancel();

            while (queue.Count > 0)
            {
                _ = await queue.ReceiveAsync(CancellationToken.None).ConfigureAwait(false);
            }

            // assert
            await AssertTaskReturnsAsync(task1, true).ConfigureAwait(false);
            await AssertTaskIsCancelledAsync(task2).ConfigureAwait(false);
            await AssertTaskReturnsAsync(task3, true).ConfigureAwait(false);
        }

        [Fact]
        public async Task CompletingQueueWillNotStopProcessingAddedAndPosponedItemsInQueue()
        {
            // arrange
            InitCancellationTokens(2);

            // act
            var task1 = queue.SendAsync(1, ctsSources[0].Token);
            queue.Complete();

            var task2 = queue.SendAsync(2, ctsSources[1].Token);

            while (queue.Count > 0)
            {
                _ = await queue.ReceiveAsync(CancellationToken.None).ConfigureAwait(false);
            }

            // assert
            await AssertTaskReturnsAsync(task1, true).ConfigureAwait(false);
            await AssertTaskReturnsAsync(task2, false).ConfigureAwait(false);
        }

        [Fact]
        public async Task SendingItemToCompletedQueueResultsInFalseTaskTest()
        {
            // arrange

            // act
            queue.Complete();
            var result = await queue.SendAsync(3).ConfigureAwait(false);

            // assert
            result.Should().Be(false);
        }

        [Fact]
        public async Task OutputAvailableAsyncReturnsFalseOnCompletion()
        {
            // arrange

            // act
            queue.Complete();
            var result = await queue.OutputAvailableAsync().ConfigureAwait(false);

            // assert
            result.Should().Be(false);
        }

        [Fact]
        public async Task OutputAvailableAsyncReturnsTrueWhenItemAdded()
        {
            // arrange

            // act
            await queue.SendAsync(1).ConfigureAwait(false);
            var result = await queue.OutputAvailableAsync().ConfigureAwait(false);

            // assert
            result.Should().Be(true);
        }

        private static async Task AssertTaskReturnsAsync(Task<bool> task, bool expectedResult)
        {
            var result = await task.ConfigureAwait(false);
            result.Should().Be(expectedResult);
        }

        private static async Task AssertTaskIsCancelledAsync(Task<bool> task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            throw new Exception("Task wasn't cancelled.");
        }

        private void InitCancellationTokens(int count)
        {
            ctsSources.Clear();
            for (var i = 0; i < count; i++)
            {
                ctsSources.Add(new CancellationTokenSource());
            }
        }
    }
}

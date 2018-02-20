namespace EagleEye.ExifToolWrapper.Test.BufferBlockExperiments
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    using FluentAssertions;

    using Xunit;

    public class BufferBlockExperiments
    {
        private readonly CancellationTokenSource _ctsQueue;
        private readonly BufferBlock<int> _queue;
        private readonly List<CancellationTokenSource> _ctsSources;

        public BufferBlockExperiments()
        {
            _ctsQueue = new CancellationTokenSource();
            _queue = new BufferBlock<int>(new DataflowBlockOptions
                                              {
                                                  BoundedCapacity = 1,
                                                  CancellationToken = _ctsQueue.Token,
                                                  EnsureOrdered = true,
                                                  TaskScheduler = TaskScheduler.Current
                                              });

            _ctsSources = new List<CancellationTokenSource>();
        }

        [Fact]
        public async Task CancelQueueWillResultInFinshedTasksWithoutProcessingThemTest()
        {
            // arrange
            InitCancellationTokens(3);
            var task1 = _queue.SendAsync(1, _ctsSources[0].Token);
            var task2 = _queue.SendAsync(2, _ctsSources[1].Token);
            var task3 = _queue.SendAsync(3, _ctsSources[2].Token);

            // act
            _ctsQueue.Cancel();

            // assert
            // the first was already in the queue and is therefore true?!
            await AssertTaskReturnsAsync(task1, true).ConfigureAwait(false);

            // the other two were posponed and therefore return false!?
            await AssertTaskReturnsAsync(task2, false).ConfigureAwait(false);
            await AssertTaskReturnsAsync(task3, false).ConfigureAwait(false);
        }


        [Fact]
        public async Task BufferBlockWithBoundedCapacityOfOneShouldRespectOrderTest()
        {
            // arrange
            const int INDEX_TO_CANCEL = 55;
            const int SIZE = 1000;
            InitCancellationTokens(SIZE);

            // act
            for (var index = 0; index < SIZE; index++)
            {
                _ = _queue.SendAsync(index, _ctsSources[index].Token);
            }

            _ctsSources[INDEX_TO_CANCEL].Cancel();

            // assert
            for (var index = 0; index < INDEX_TO_CANCEL; index++)
            {
                var number = await _queue.ReceiveAsync(CancellationToken.None).ConfigureAwait(false);
                number.Should().Be(index);
            }

            for (var index = INDEX_TO_CANCEL + 1; index < SIZE; index++)
            {
                var number = await _queue.ReceiveAsync(CancellationToken.None).ConfigureAwait(false);
                number.Should().Be(index);
            }
        }

        [Fact]
        public async Task SendItemThatIsCancelledBeforeAddedToQueueWillResultInCancelledTaskTest()
        {
            // arrange
            InitCancellationTokens(3);

            // act
            var task1 = _queue.SendAsync(1, _ctsSources[0].Token);
            var task2 = _queue.SendAsync(2, _ctsSources[1].Token);
            var task3 = _queue.SendAsync(3, _ctsSources[2].Token);

            _ctsSources[1].Cancel();

            while (_queue.Count > 0)
            {
                _ = await _queue.ReceiveAsync(CancellationToken.None).ConfigureAwait(false);
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
            var task1 = _queue.SendAsync(1, _ctsSources[0].Token);
            _queue.Complete();

            var task2 = _queue.SendAsync(2, _ctsSources[1].Token);

            while (_queue.Count > 0)
            {
                _ = await _queue.ReceiveAsync(CancellationToken.None).ConfigureAwait(false);
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
            _queue.Complete();
            var result = await _queue.SendAsync(3).ConfigureAwait(false);

            // assert
            result.Should().Be(false);
        }

        [Fact]
        public async Task OutputAvailableAsyncReturnsFalseOnCompletion()
        {
            // arrange
            var cts = new CancellationTokenSource(30);

            // act
            var task = Task.Run(
                                async () =>
                                {
                                    await Task.Delay(10).ConfigureAwait(false);
                                    _queue.Complete();
                                },
                                CancellationToken.None);
            var result = await _queue.OutputAvailableAsync(cts.Token).ConfigureAwait(false);

            // assert
            result.Should().Be(false);
            await task.ConfigureAwait(false);
        }


        [Fact]
        public async Task OutputAvailableAsyncReturnsTrueWhenItemAdded()
        {
            // arrange
            var cts = new CancellationTokenSource(30);

            // act
            var task = Task.Run(
                                async () =>
                                {
                                    await Task.Delay(10).ConfigureAwait(false);
                                    await _queue.SendAsync(1).ConfigureAwait(false);
                                },
                                CancellationToken.None);
            var result = await _queue.OutputAvailableAsync(cts.Token).ConfigureAwait(false);

            // assert
            result.Should().Be(true);
            await task.ConfigureAwait(false);
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

            throw new Exception("Task wasnt cancelled.");
        }

        private void InitCancellationTokens(int count)
        {
            _ctsSources.Clear();
            for (var i = 0; i < count; i++)
            {
                _ctsSources.Add(new CancellationTokenSource());
            }
        }
    }
}
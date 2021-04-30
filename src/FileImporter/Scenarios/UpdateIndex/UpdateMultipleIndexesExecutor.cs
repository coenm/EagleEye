namespace EagleEye.FileImporter.Scenarios.UpdateIndex
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Dawn;
    using JetBrains.Annotations;
    using NLog;

    [UsedImplicitly]
    public class UpdateMultipleIndexesExecutor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IUpdateIndexExecutor updateIndexExecutor;

        public UpdateMultipleIndexesExecutor([NotNull] IUpdateIndexExecutor updateIndexExecutor)
        {
            Guard.Argument(updateIndexExecutor, nameof(updateIndexExecutor)).NotNull();
            this.updateIndexExecutor = updateIndexExecutor;
        }

        public async Task ExecuteAsync([NotNull] IEnumerable<string> files, [CanBeNull] IProgress<FileProcessingProgress> singleFileProgress, CancellationToken ct = default)
        {
            Guard.Argument(files, nameof(files)).NotNull();

            var filesArray = files.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();

            if (filesArray.Length == 0)
                return;

            var fromSeconds = TimeSpan.FromSeconds(120); // todo
            var maxDegree = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0));

            // if (maxDegree > 1)
                // maxDegree = 1;

            Logger.Info($"Start processing using {maxDegree} task");

            if (maxDegree <= 1)
            {
                foreach (var file in filesArray)
                {
                    using var cts = new CancellationTokenSource(fromSeconds);
                    using var combinedCt = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);
                    await ProcessSingleAsync(file, singleFileProgress, combinedCt.Token).ConfigureAwait(false);
                }
            }
            else
            {
                Parallel.ForEach(
                                 filesArray,
                                 new ParallelOptions { MaxDegreeOfParallelism = maxDegree },
                                 file =>
                                 {
                                     using var cts = new CancellationTokenSource(fromSeconds);
                                     using var combinedCt = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);
                                     ProcessSingleAsync(file, singleFileProgress, combinedCt.Token).ConfigureAwait(false).GetAwaiter().GetResult();
                                 });
            }
        }

        private async Task ProcessSingleAsync([NotNull] string file, [CanBeNull] IProgress<FileProcessingProgress> progress, CancellationToken ct)
        {
            try
            {
                progress?.Report(new FileProcessingProgress(file, 0, int.MaxValue, "start", ProgressState.Busy));
                await Task.Delay(500).ConfigureAwait(false); // stupid, make sure progress bar has been spawned etc.
                await updateIndexExecutor.HandleAsync(file, progress, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Logger.Error("Could not UpdateImporteImage due to timeout.");
                progress?.Report(new FileProcessingProgress(file, int.MaxValue, int.MaxValue, "Operation Canceled", ProgressState.Failure));
            }
            catch (Exception e)
            {
                Logger.Warn(e.Message);
                progress?.Report(new FileProcessingProgress(file, int.MaxValue, int.MaxValue, $"Exception {e.Message}", ProgressState.Failure));
            }
        }
    }
}

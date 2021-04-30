namespace EagleEye.FileStamper.Console.Scenarios.FixAndUpdateImportImages
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
    public class UpdateMultipleImagesExecutor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IUpdateImportImageCommandHandler updateImportImageCommandHandler;

        public UpdateMultipleImagesExecutor([NotNull] IUpdateImportImageCommandHandler updateImportImageCommandHandler)
        {
            Guard.Argument(updateImportImageCommandHandler, nameof(updateImportImageCommandHandler)).NotNull();
            this.updateImportImageCommandHandler = updateImportImageCommandHandler;
        }

        public async Task ExecuteAsync([NotNull] IEnumerable<string> files, [CanBeNull] IProgress<FileProcessingProgress> singleFileProgress, CancellationToken ct = default)
        {
            Guard.Argument(files, nameof(files)).NotNull();

            var filesArray = files.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();

            if (filesArray.Length == 0)
                return;

            var fromSeconds = TimeSpan.FromSeconds(25);
            var maxDegree = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0));

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
                progress?.Report(new FileProcessingProgress(file, 1, 2, "Start", ProgressState.Busy));

                await updateImportImageCommandHandler.HandleAsync(file, ct).ConfigureAwait(false);

                progress?.Report(new FileProcessingProgress(file, 2, 2, "Finished", ProgressState.Success));
            }
            catch (OperationCanceledException)
            {
                Logger.Error("Could not UpdateImporteImage due to timeout.");
                progress?.Report(new FileProcessingProgress(file, 2, 2, "Operation Canceled", ProgressState.Failure));
            }
            catch (Exception e)
            {
                Logger.Warn(e.Message);
                progress?.Report(new FileProcessingProgress(file, 2, 2, $"Exception {e.Message}", ProgressState.Failure));
            }
        }
    }
}

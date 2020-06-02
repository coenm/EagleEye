namespace EagleEye.FileImporter.Scenarios.UpdateIndex
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    public interface IUpdateIndexExecutor
    {
        Task HandleAsync([NotNull] string filename, [CanBeNull] IProgress<FileProcessingProgress> progress, CancellationToken ct = default);
    }
}

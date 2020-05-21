namespace EagleEye.FileImporter.Scenarios.FixAndUpdateImportImages
{
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    public interface IUpdateImportImageCommandHandler
    {
        Task HandleAsync([NotNull] string filename, CancellationToken ct = default);
    }
}

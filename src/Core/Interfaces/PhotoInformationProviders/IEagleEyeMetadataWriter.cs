namespace EagleEye.Core.Interfaces.PhotoInformationProviders
{
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Core.EagleEyeXmp;
    using JetBrains.Annotations;

    public interface IEagleEyeMetadataWriter
    {
        Task WriteAsync([NotNull] string filename, [NotNull] EagleEyeMetadata metadata, bool overwriteOriginal, CancellationToken ct = default);
    }
}

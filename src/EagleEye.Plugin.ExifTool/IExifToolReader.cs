namespace EagleEye.ExifTool
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;
    using Newtonsoft.Json.Linq;

    internal interface IExifToolReader : IAsyncDisposable
    {
        Task<JObject> GetMetadataAsync([NotNull] string filename, CancellationToken ct = default);
    }
}

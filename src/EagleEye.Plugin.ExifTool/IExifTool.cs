namespace EagleEye.ExifTool
{
    using System;
    using System.Threading.Tasks;

    using JetBrains.Annotations;
    using Newtonsoft.Json.Linq;

    internal interface IExifTool : IAsyncDisposable
    {
        Task<JObject> GetMetadataAsync([NotNull] string filename);
    }
}

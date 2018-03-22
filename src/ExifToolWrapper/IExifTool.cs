namespace EagleEye.ExifToolWrapper
{
    using System;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Newtonsoft.Json.Linq;

    public interface IExifTool : IDisposable
    {
        Task<JObject> GetMetadataAsync([NotNull] string filename);
    }
}
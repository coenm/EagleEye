namespace EagleEye.ExifToolWrapper
{
    using System;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    public interface IExifTool : IDisposable
    {
        Task<JObject> GetMetadataAsync(string filename);
    }
}
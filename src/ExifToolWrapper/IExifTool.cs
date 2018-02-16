namespace EagleEye.ExifToolWrapper
{
    using System;
    using System.Threading.Tasks;

    public interface IExifTool : IDisposable
    {
        Task<dynamic> GetMetadataAsync(string filename);
    }
}
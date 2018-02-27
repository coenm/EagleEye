namespace EagleEye.ExifToolWrapper.ExifToolSimplified
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IExifToolSimple
    {
        // Initialize and start exiftool
        void Init();

        Task<string> ExecuteAsync(IEnumerable<string> args, CancellationToken ct = default(CancellationToken));

        Task DisposeAsync(CancellationToken ct = default(CancellationToken));
    }
}
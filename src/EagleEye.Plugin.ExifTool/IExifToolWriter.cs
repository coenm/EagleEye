namespace EagleEye.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    internal interface IExifToolWriter : IAsyncDisposable
    {
        Task WriteAsync([NotNull] string filename, IEnumerable<string> exiftoolArgs, CancellationToken ct = default);
    }
}

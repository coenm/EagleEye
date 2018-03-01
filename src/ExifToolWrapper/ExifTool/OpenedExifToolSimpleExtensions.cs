namespace EagleEye.ExifToolWrapper.ExifTool
{
    using System.Threading;
    using System.Threading.Tasks;

    public static class OpenedExifToolSimpleExtensions
    {
        public static Task<string> GetVersionAsync(this OpenedExifTool @this, CancellationToken ct = default(CancellationToken))
        {
            return @this.ExecuteAsync(new[] { "-ver" }, ct);
        }

        public static Task<string> ExecuteAsync(this OpenedExifTool @this, string singleArg, CancellationToken ct = default(CancellationToken))
        {
            return @this.ExecuteAsync(new[] { singleArg }, ct);
        }
    }
}
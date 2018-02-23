namespace EagleEye.ExifToolWrapper.ExifToolSimplified
{
    using System.Threading;
    using System.Threading.Tasks;

    public static class OpenedExifToolSimpleExtensions
    {
        public static Task<string> GetVersionAsync(this OpenedExifToolSimple @this, CancellationToken ct = default(CancellationToken))
        {
            return @this.ExecuteAsync(new[] { "-ver" }, ct);
        }

        public static Task<string> ExecuteAsync(this OpenedExifToolSimple @this, string singleArg, CancellationToken ct = default(CancellationToken))
        {
            return @this.ExecuteAsync(new[] { singleArg }, ct);
        }
    }
}
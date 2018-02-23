namespace EagleEye.ExifToolWrapper.ExifToolSimplified
{
    using System.Threading.Tasks;

    using Medallion.Shell;

    public interface IMedallionShell
    {
        Task<CommandResult> Task { get; }
        void Kill();
        Task WriteLineAsync(string text);
        void WriteLine(string s);
    }
}
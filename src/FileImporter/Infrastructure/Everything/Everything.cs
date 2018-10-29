namespace EagleEye.FileImporter.Infrastructure.Everything
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.FileImporter.Indexing;

    public class Everything
    {
        private const string EverythingExe = "C:\\Program Files\\Everything\\Everything.exe";
        private const string EverythingOr = "|";
        private const string Escape = "\"";
        private const string StartEnd = "\"\"\"";

        public Task Show(IEnumerable<ImageData> files)
        {
            var files2 = files.Select(f => f.Identifier);
            var search = string.Join(EverythingOr, files2);
            var args = "-s " + Escape + "filelist:" + StartEnd + search + StartEnd + " " + Escape;

            var p = new Process
            {
                StartInfo = new ProcessStartInfo(EverythingExe, args),
            };
            p.Start();

            return Task.CompletedTask;
        }
    }
}

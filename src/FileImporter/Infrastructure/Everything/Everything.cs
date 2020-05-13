namespace EagleEye.FileImporter.Infrastructure.Everything
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class Everything
    {
        private const string EverythingExe = "C:\\Program Files\\Everything\\Everything.exe";
        private const string EverythingOr = "|";
        private const string Escape = "\"";
        private const string StartEnd = "\"\"\"";

        public Task Show(IEnumerable<string> files)
        {
            var search = string.Join(EverythingOr, files);
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

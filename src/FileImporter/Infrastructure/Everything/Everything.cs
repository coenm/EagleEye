using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EagleEye.FileImporter.Indexing;

namespace EagleEye.FileImporter.Infrastructure.Everything
{
    public class Everything
    {
        private const string EverythingExe = "C:\\Program Files\\Everything\\Everything.exe";
        private const string Or = "|";
        private const string Escape = "\"";
        private const string StartEnd = "\"\"\"";


        public Task Show(List<ImageData> files)
        {
            var files2 = files.Select(f=>f.Identifier);
            var search = string.Join(Or, files2);

            Process p = new Process();
            string args = "-s " + Escape + "filelist:" + StartEnd + search + StartEnd + " " + Escape;

            p.StartInfo = new ProcessStartInfo(EverythingExe, args);
            p.Start();

            return Task.CompletedTask;
        }
    }
}
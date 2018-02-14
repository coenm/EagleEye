namespace EagleEye.FileImporter.Infrastructure.Everything
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.FileImporter.Indexing;

    public class Everything
    {
        private const string EVERYTHING_EXE = "C:\\Program Files\\Everything\\Everything.exe";
        private const string EVERYTHING_OR = "|";
        private const string ESCAPE = "\"";
        private const string START_END = "\"\"\"";


        public Task Show(IEnumerable<ImageData> files)
        {
            var files2 = files.Select(f => f.Identifier);
            var search = string.Join(EVERYTHING_OR, files2);
            var args = "-s " + ESCAPE + "filelist:" + START_END + search + START_END + " " + ESCAPE;

            var p = new Process
            {
                StartInfo = new ProcessStartInfo(EVERYTHING_EXE, args)
            };
            p.Start();

            return Task.CompletedTask;
        }
    }
}
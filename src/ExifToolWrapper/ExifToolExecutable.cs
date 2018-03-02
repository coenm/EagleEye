namespace EagleEye.ExifToolWrapper
{
    using System.Runtime.InteropServices;
    using System.Text;

    public static class ExifToolExecutable
    {
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);


        public static string NewLine
        {
            get
            {
                if (IsWindows)
                    return "\r\n";
                return "\n";
            }
        }
        public static byte[] NewLineBytes => Encoding.ASCII.GetBytes(NewLine);

        public static string GetExecutableName()
        {
            if (IsLinux)
                return "exiftool";
            return "exiftool.exe";
        }
    }
}
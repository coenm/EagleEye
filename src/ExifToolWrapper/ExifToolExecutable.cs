namespace EagleEye.ExifToolWrapper
{
    using System.Runtime.InteropServices;
    using System.Text;

    public static class ExifToolExecutable
    {
        public const string WINDOWS_EOL = "\r\n";

        public const string LINUX_EOL = "\n";

        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static string ConvertWindowsToOsString(this string input)
        {
            if (IsWindows)
                return input;

            return input.Replace(WINDOWS_EOL, LINUX_EOL);
        }

        public static string NewLine
        {
            get
            {
                if (IsWindows)
                    return WINDOWS_EOL;
                return LINUX_EOL;
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
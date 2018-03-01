namespace EagleEye.ExifToolWrapper.Test
{
    using System.Runtime.InteropServices;

    internal static class ExifToolExecutable
    {
        private static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static string GetExecutableName()
        {
            if (IsLinux)
                return "exiftool";
            return "exiftool.exe";
        }
    }
}
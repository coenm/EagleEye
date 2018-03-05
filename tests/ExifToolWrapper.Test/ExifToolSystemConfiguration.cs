namespace EagleEye.ExifToolWrapper.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using EagleEye.TestImages;

    internal static class ExifToolSystemConfiguration
    {
        private const string EXIFTOOL_VERSION = "EXIFTOOL_VERSION";
        private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
        private static readonly string _embeddedResourceNs = typeof(ExifToolSystemConfiguration).Namespace;
        private static readonly Lazy<string> GetConfigExiftoolVersionImpl = new Lazy<string>(GetConfiguredExiftoolVersion);
        private static readonly Lazy<string> GetExifToolExecutableImpl = new Lazy<string>(GetExifToolExecutable);


        public static string ConfiguredVersion => GetConfigExiftoolVersionImpl.Value;

        public static string ExifToolExecutable => GetExifToolExecutableImpl.Value;


        private static string GetExifToolExecutable()
        {
            var osFilename = ExifToolWrapper.ExifToolExecutable.GetExecutableName();

            // first try to grab local Exiftool, otherwise assume global exiftool
            var fullFilename = TestEnvironment.GetFullPath("tools", osFilename);
            if (File.Exists(fullFilename))
                return fullFilename;

            return osFilename;
        }

        private static string GetConfiguredExiftoolVersion()
        {
            using (var stream = OpenRead())
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        private static Stream OpenRead()
        {
            var x = _assembly.GetManifestResourceNames().ToArray();
            return _assembly.GetManifestResourceStream(_embeddedResourceNs + "." + EXIFTOOL_VERSION);
        }
    }
}
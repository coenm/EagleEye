namespace EagleEye.TestImages
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public static class TestEnvironment
    {
        private const string SOLUTION_FILE_NAME = "EagleEye.sln";
        private const string INPUT_IMAGES_RELATIVE_PATH = @"Images\data\";
        private static readonly Lazy<string> LazySolutionDirectoryFullPath = new Lazy<string>(GetSolutionDirectoryFullPathImpl);
        private static readonly Lazy<bool> RunsOnContinuousIntegration = new Lazy<bool>(IsContinuousIntegrationImpl);

        /// <summary>
        /// Gets a value indicating whether test execution runs on CI.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static bool RunsOnCI => RunsOnContinuousIntegration.Value;

        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Gets the correct full path to the Images directory.
        /// </summary>
        public static string InputImagesDirectoryFullPath => GetFullPath(INPUT_IMAGES_RELATIVE_PATH);

        private static string SolutionDirectoryFullPath => LazySolutionDirectoryFullPath.Value;

        /// <summary>
        /// Convert relative path to full path based on solution directory (directory containing the sln file).
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public static string GetFullPath(params string[] relativePath)
        {
            var paths = new[] { SolutionDirectoryFullPath }.Concat(relativePath).ToArray();
            return Path
                   .Combine(paths)
                   .Replace('\\', Path.DirectorySeparatorChar);
        }

        /// <summary> Read image from relative filename for testing purposes </summary>
        /// <param name="relativeFilename">Filename relative to 'solution directory' + 'images/data/'. </param>
        /// <returns>FileStream or throws exception</returns>
        public static FileStream ReadRelativeImageFile(string relativeFilename)
        {
            var fullFilename = GetFullPath(INPUT_IMAGES_RELATIVE_PATH, relativeFilename);
            return File.OpenRead(fullFilename);
        }

        private static bool IsContinuousIntegrationImpl()
        {
            return bool.TryParse(Environment.GetEnvironmentVariable("CI"), out var isCi) && isCi;
        }

        private static string GetSolutionDirectoryFullPathImpl()
        {
            var assemblyLocation = typeof(TestEnvironment).GetTypeInfo().Assembly.Location;

            var assemblyFile = new FileInfo(assemblyLocation);

            var directory = assemblyFile.Directory;

            if (directory == null)
                throw new Exception($"Unable to find solution directory from '{assemblyLocation}'!");

            while (!directory.EnumerateFiles(SOLUTION_FILE_NAME).Any())
            {
                try
                {
                    directory = directory.Parent;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to find solution directory from '{assemblyLocation}' because of {ex.GetType().Name}!",ex);
                }

                if (directory == null)
                    throw new Exception($"Unable to find solution directory from '{assemblyLocation}'!");
            }

            return directory.FullName;
        }
    }
}

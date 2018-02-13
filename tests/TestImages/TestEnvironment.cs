using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TestImages
{
    public static class TestEnvironment
    {
        private const string SolutionFileName = "EagleEye.sln";

        private const string InputImagesRelativePath = @"Images\data\";

        private static readonly Lazy<string> LazySolutionDirectoryFullPath = new Lazy<string>(GetSolutionDirectoryFullPathImpl);

        private static readonly Lazy<bool> RunsOnContinuousIntegration = new Lazy<bool>(IsContinuousIntegrationImpl);

        // ReSharper disable once InconsistentNaming

        /// <summary>
        /// Gets a value indicating whether test execution runs on CI.
        /// </summary>
        public static bool RunsOnCI => RunsOnContinuousIntegration.Value;

        private static string SolutionDirectoryFullPath => LazySolutionDirectoryFullPath.Value;

        private static string GetSolutionDirectoryFullPathImpl()
        {
            string assemblyLocation = typeof(TestEnvironment).GetTypeInfo().Assembly.Location;

            var assemblyFile = new FileInfo(assemblyLocation);

            var directory = assemblyFile.Directory;

            while (!directory.EnumerateFiles(SolutionFileName).Any())
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

        private static bool IsContinuousIntegrationImpl()
        {
            return bool.TryParse(Environment.GetEnvironmentVariable("CI"), out var isCi) && isCi;
        }

        /// <summary>
        /// Convert relative path to full path based on solution directory (directory containing the sln file).
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public static string GetFullPath(params string[] relativePath)
        {
            var paths = new[] {SolutionDirectoryFullPath}.Concat(relativePath).ToArray();
            return Path
                .Combine(paths)
                .Replace('\\', Path.DirectorySeparatorChar);
        }
        
        /// <summary> Read image from relative filename for testing purposes </summary>
        /// <param name="relativeFilename">Filename relative to 'solution directory' + 'images/data/'. </param>
        /// <returns>FileStream or throws exception</returns>
        public static FileStream ReadRelativeImageFile(string relativeFilename)
        {
            var fullFilename = GetFullPath(InputImagesRelativePath, relativeFilename);
            return File.OpenRead(fullFilename);
        }

        /// <summary>
        /// Gets the correct full path to the Images directory.
        /// </summary>
        public static string InputImagesDirectoryFullPath => GetFullPath(InputImagesRelativePath);
    }
}

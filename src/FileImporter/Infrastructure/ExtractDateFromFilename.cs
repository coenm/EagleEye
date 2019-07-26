namespace EagleEye.FileImporter.Infrastructure
{
    using System;

    using EagleEye.DirectoryStructure.PhotoProvider;

    public static class ExtractDateFromFilename
    {
        public static DateTime? TryGetFromFilename(string filename)
        {
            var result = new MobileFilenameDateTimeProvider()
                         .ProvideAsync(filename, null)
                         .GetAwaiter()
                         .GetResult();
            if (result == null)
                return null;

            return new DateTime(result.Value.Year, result.Value.Month, result.Value.Day, 0, 0, 0);
        }
    }
}

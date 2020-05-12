namespace EagleEye.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Dawn;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.ExifTool.EagleEyeXmp;
    using EagleEye.ExifTool.PhotoProvider;
    using JetBrains.Annotations;
    using SimpleInjector;

    [UsedImplicitly]
    internal class ExifToolPlugin : IEagleEyePlugin
    {
        public const string ConfigKeyExiftoolPluginFullConfigFile = "EXIFTOOL_PLUGIN_FULL_CONFIG_FILE";
        public const string ConfigKeyExiftoolPluginFullExe = "EXIFTOOL_PLUGIN_FULL_EXE";

        public string Name => nameof(ExifToolPlugin);

        public void EnablePlugin([NotNull] Container container, [CanBeNull] IReadOnlyDictionary<string, object> settings)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            string exifToolConfigFile = null;

            if (TryGetExifToolConfigFromConfig(ref settings, out var foundExifToolConfigFile))
                exifToolConfigFile = foundExifToolConfigFile;
            else if (TryGetExifToolConfigFromConvention(out foundExifToolConfigFile))
                exifToolConfigFile = foundExifToolConfigFile;

            if (!TryGetExifToolExeFromConfig(ref settings, out var foundExe))
                foundExe = ExifToolExecutable.GetExecutableName();

            container.Register<IExifToolConfig>(() => new StaticExiftoolConfig(foundExe, exifToolConfigFile), Lifestyle.Singleton);
            container.Register<IExifToolArguments>(() => new StaticExifToolArguments(StaticExifToolArguments.DefaultArguments), Lifestyle.Singleton);

            container.Register<IExifToolWriter, ExifToolAdapter>(Lifestyle.Singleton);

            container.Register<IExifToolReader, ExifToolAdapter>(Lifestyle.Singleton);
            // container.RegisterDecorator<IExifToolReader, ExifToolCacheDecorator>(Lifestyle.Singleton);

            container.Register<IEagleEyeMetadataProvider, EagleEyeMetadataProvider>();
            container.Register<IEagleEyeMetadataWriter, EagleEyeMetadataWriter>();

            container.Collection.Append(typeof(IPhotoDateTimeTakenProvider), typeof(ExifToolDateTakenProvider));
            container.Collection.Append(typeof(IPhotoLocationProvider), typeof(ExifToolGpsProvider));
            container.Collection.Append(typeof(IPhotoLocationProvider), typeof(ExifToolLocationProvider));
            container.Collection.Append(typeof(IPhotoPersonProvider), typeof(ExifToolPersonsProvider));
            container.Collection.Append(typeof(IPhotoTagProvider), typeof(ExifToolTagsProvider));
        }

        private static bool TryGetExifToolConfigFromConvention(out string output)
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            string exifToolConfigFile = Path.Combine(dir, "EagleEye.ExifTool_config");
            if (!File.Exists(exifToolConfigFile))
            {
                output = null;
                return false;
            }

            output = exifToolConfigFile;
            return true;
        }

        private static bool TryGetExifToolConfigFromConfig(ref IReadOnlyDictionary<string, object> settings, out string output)
        {
            return TryGetStringFromConfigDictionary(ref settings, ConfigKeyExiftoolPluginFullConfigFile, out output);
        }

        private static bool TryGetExifToolExeFromConfig(ref IReadOnlyDictionary<string, object> settings, out string exe)
        {
            return TryGetStringFromConfigDictionary(ref settings, ConfigKeyExiftoolPluginFullExe, out exe);
        }

        private static bool TryGetStringFromConfigDictionary(ref IReadOnlyDictionary<string, object> settings, string key, out string output)
        {
            output = null;

            if (settings == null)
                return false;

            if (!settings.TryGetValue(key, out object value))
                return false;

            if (value is null)
                return false;

            if (!(value is string stringValue))
                return false;

            output = stringValue;
            return true;
        }
    }
}

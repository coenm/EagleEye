namespace EagleEye.ExifTool
{
    using System;
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
        public string Name => nameof(ExifToolPlugin);

        public void EnablePlugin([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            // Assembly.GetExecutingAssembly().
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var exifToolConfigFile = Path.Combine(dir, "EagleEye.ExifTool_config");
            // if (!File.Exists(exifToolConfigFile))
            exifToolConfigFile = null;

            container.Register<IExifToolConfig>(() => new StaticExiftoolConfig(ExifToolExecutable.GetExecutableName(), exifToolConfigFile), Lifestyle.Singleton); // todo coenm fix this
            container.Register<IExifToolArguments>(() => new StaticExifToolArguments(StaticExifToolArguments.DefaultArguments), Lifestyle.Singleton);

            container.Register<IExifToolWriter, ExifToolAdapter>(Lifestyle.Singleton);

            container.Register<IExifToolReader, ExifToolAdapter>(Lifestyle.Singleton);
            container.RegisterDecorator<IExifToolReader, ExifToolCacheDecorator>(Lifestyle.Singleton);

            container.Register<IEagleEyeMetadataProvider, EagleEyeMetadataProvider>();
            container.Register<IEagleEyeMetadataWriter, EagleEyeMetadataWriter>();

            container.Collection.Append(typeof(IPhotoDateTimeTakenProvider), typeof(ExifToolDateTakenProvider));
            container.Collection.Append(typeof(IPhotoLocationProvider), typeof(ExifToolGpsProvider));
            container.Collection.Append(typeof(IPhotoLocationProvider), typeof(ExifToolLocationProvider));
            container.Collection.Append(typeof(IPhotoPersonProvider), typeof(ExifToolPersonsProvider));
            container.Collection.Append(typeof(IPhotoTagProvider), typeof(ExifToolTagsProvider));
        }
    }
}

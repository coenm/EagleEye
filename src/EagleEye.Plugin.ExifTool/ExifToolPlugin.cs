namespace EagleEye.ExifTool
{
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

            container.Register<IExifToolConfig>(() => new StaticExiftoolConfig(ExifToolExecutable.GetExecutableName()), Lifestyle.Singleton); // todo coenm fix this
            container.Register<IExifTool, ExifToolAdapter>(Lifestyle.Singleton);
            container.RegisterDecorator<IExifTool, ExifToolCacheDecorator>(Lifestyle.Singleton);

            container.Register<IEagleEyeMetadataProvider, EagleEyeMetadataProvider>();

            container.Collection.Append(typeof(IPhotoDateTimeTakenProvider), typeof(ExifToolDateTakenProvider));
            container.Collection.Append(typeof(IPhotoLocationProvider), typeof(ExifToolGpsProvider));
            container.Collection.Append(typeof(IPhotoLocationProvider), typeof(ExifToolLocationProvider));
            container.Collection.Append(typeof(IPhotoPersonProvider), typeof(ExifToolPersonsProvider));
            container.Collection.Append(typeof(IPhotoTagProvider), typeof(ExifToolTagsProvider));
        }
    }
}

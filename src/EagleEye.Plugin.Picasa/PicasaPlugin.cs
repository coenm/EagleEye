namespace EagleEye.Picasa
{
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.Picasa.PhotoProvider;
    using Dawn;
    using JetBrains.Annotations;
    using SimpleInjector;

    [UsedImplicitly]
    internal class PicasaPlugin : IEagleEyePlugin
    {
        public string Name => nameof(PicasaPlugin);

        public void EnablePlugin([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            container.Register<IPicasaService, PicasaService>(Lifestyle.Singleton);
            container.Collection.Append(typeof(IPhotoPersonProvider), typeof(PicasaPersonProvider));
        }
    }
}

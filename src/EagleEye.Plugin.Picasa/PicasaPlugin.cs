namespace EagleEye.Picasa
{
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.Picasa.PhotoProvider;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using SimpleInjector;

    [UsedImplicitly]
    internal class PicasaPlugin : IEagleEyePlugin
    {
        public string Name => nameof(PicasaPlugin);

        public void EnablePlugin([NotNull] Container container)
        {
            Guard.NotNull(container, nameof(container));

            container.Register<IPicasaService, PicasaService>();
            container.Collection.Append(typeof(IPhotoPersonProvider), typeof(PicasaPersonProvider));
        }
    }
}

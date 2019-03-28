namespace EagleEye.Picasa
{
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.Picasa.PhotoProvider;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;
    using SimpleInjector;

    [UsedImplicitly]
    internal class PicasaPlugin : IEagleEyePlugin
    {
        public string Name => nameof(PicasaPlugin);

        public void EnablePlugin([NotNull] Container container)
        {
            Helpers.Guards.Guard.NotNull(container, nameof(container));

            container.Register<IPicasaService, PicasaService>(Lifestyle.Singleton);
            container.Collection.Append(typeof(IPhotoPersonProvider), typeof(PicasaPersonProvider));
        }
    }
}

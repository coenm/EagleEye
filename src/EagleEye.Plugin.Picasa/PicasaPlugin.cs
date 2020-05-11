namespace EagleEye.Picasa
{
    using System.Collections.Generic;

    using Dawn;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.Picasa.PhotoProvider;
    using JetBrains.Annotations;
    using SimpleInjector;

    [UsedImplicitly]
    internal class PicasaPlugin : IEagleEyePlugin
    {
        public string Name => nameof(PicasaPlugin);

        public void EnablePlugin([NotNull] Container container, [CanBeNull] IReadOnlyDictionary<string, object> settings)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            container.Register<IPicasaService, PicasaService>(Lifestyle.Singleton);
            container.Collection.Append(typeof(IPhotoPersonProvider), typeof(PicasaPersonProvider));
        }
    }
}

namespace EagleEye.ExifTool
{
    using EagleEye.Core.Interfaces.Module;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using SimpleInjector;

    internal class ExifToolPlugin : IEagleEyePlugin
    {
        public string Name => "PicasaPlugin";

        public void EnablePlugin([NotNull] Container container)
        {
            Guard.NotNull(container, nameof(container));

//            container.Register<IPicasaService, PicasaService>();
//            container.Collection.Append(typeof(IPhotoPersonProvider), typeof(PicasaPersonProvider));
        }
    }
}

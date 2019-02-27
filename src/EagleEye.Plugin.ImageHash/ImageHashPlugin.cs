namespace EagleEye.ImageHash
{
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.ImageHash.PhotoProvider;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using SimpleInjector;

    [UsedImplicitly]
    internal class ImageHashPlugin : IEagleEyePlugin
    {
        public string Name => nameof(ImageHashPlugin);

        public void EnablePlugin([NotNull] Container container)
        {
            Guard.NotNull(container, nameof(container));

            container.Collection.Append(typeof(IPhotoHashProvider), typeof(PhotoHashProvider));
        }
    }
}

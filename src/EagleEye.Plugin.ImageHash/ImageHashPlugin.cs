namespace EagleEye.ImageHash
{
    using Dawn;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.ImageHash.PhotoProvider;
    using JetBrains.Annotations;
    using SimpleInjector;

    [UsedImplicitly]
    internal class ImageHashPlugin : IEagleEyePlugin
    {
        public string Name => nameof(ImageHashPlugin);

        public void EnablePlugin([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            container.Collection.Append(typeof(IPhotoHashProvider), typeof(PhotoHashProvider));
        }
    }
}

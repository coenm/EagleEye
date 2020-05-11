namespace EagleEye.ImageHash
{
    using System.Collections.Generic;

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

        public void EnablePlugin([NotNull] Container container, [CanBeNull] IReadOnlyDictionary<string, object> settings)
        {
            Guard.Argument(container, nameof(container)).NotNull();

            container.Collection.Append(typeof(IPhotoHashProvider), typeof(ImageSharpPhotoHashProvider));
            container.Collection.Append(typeof(IPhotoSha256HashProvider), typeof(ImageSharpPhotoSha256HashProvider));
        }
    }
}

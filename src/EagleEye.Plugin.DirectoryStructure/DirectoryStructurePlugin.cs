namespace EagleEye.DirectoryStructure
{
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.DirectoryStructure.PhotoProvider;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;
    using SimpleInjector;

    [UsedImplicitly]
    internal class DirectoryStructurePlugin : IEagleEyePlugin
    {
        public string Name => nameof(DirectoryStructurePlugin);

        public void EnablePlugin([NotNull] Container container)
        {
            Helpers.Guards.Guard.NotNull(container, nameof(container));

            container.Collection.Append(typeof(IPhotoDateTimeTakenProvider), typeof(DirectoryStructureDateTimeProvider));
        }
    }
}

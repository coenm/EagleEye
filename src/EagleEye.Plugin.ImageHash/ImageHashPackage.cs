namespace EagleEye.ImageHash
{
    using EagleEye.Core.Interfaces.Module;
    using JetBrains.Annotations;
    using SimpleInjector;
    using SimpleInjector.Packaging;

    [UsedImplicitly]
    public class ImageHashPackage : IPackage
    {
        public void RegisterServices([NotNull] Container container)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once HeuristicUnreachableCode
            // ReSharper disable once UseNullPropagation
            if (container == null)
                return;

            container.Collection.Append<IEagleEyePlugin, ImageHashPlugin>(Lifestyle.Singleton);
        }
    }
}

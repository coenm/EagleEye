namespace EagleEye.Core.Interfaces.Module
{
    using JetBrains.Annotations;
    using SimpleInjector;

    public interface IEagleEyePlugin
    {
        string Name { get; }

        void EnablePlugin([NotNull] Container container);
    }
}

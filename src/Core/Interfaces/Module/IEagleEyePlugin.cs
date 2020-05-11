namespace EagleEye.Core.Interfaces.Module
{
    using System.Collections.Generic;

    using JetBrains.Annotations;
    using SimpleInjector;

    public interface IEagleEyePlugin
    {
        string Name { get; }

        void EnablePlugin([NotNull] Container container, [CanBeNull] IReadOnlyDictionary<string, object> settings);
    }
}

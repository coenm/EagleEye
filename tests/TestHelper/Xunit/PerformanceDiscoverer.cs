// ReSharper disable once CheckNamespace
namespace EagleEye.Categories
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Xunit.Abstractions;
    using Xunit.Sdk;

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Xunit")]
    public class PerformanceDiscoverer : ITraitDiscoverer
    {
        public const string DISCOVERER_TYPE_NAME = TestHelperSettings.Namespace + nameof(PerformanceDiscoverer);

        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            yield return new KeyValuePair<string, string>("Category", "Performance");
        }
    }
}
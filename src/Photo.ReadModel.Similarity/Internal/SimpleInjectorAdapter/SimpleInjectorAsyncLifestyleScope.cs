namespace Photo.ReadModel.Similarity.Internal.SimpleInjectorAdapter
{
    using System;

    using Hangfire;
    using Helpers.Guards;
    using JetBrains.Annotations;

    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    internal class SimpleInjectorAsyncLifestyleScope : JobActivatorScope
    {
        private readonly Container container;
        private readonly Scope scope;

        public SimpleInjectorAsyncLifestyleScope([NotNull] Container container)
        {
            Guard.NotNull(container, nameof(container));

            this.container = container;
            scope = AsyncScopedLifestyle.BeginScope(container);
        }

        public override object Resolve(Type type)
        {
            return container.GetInstance(type);
        }

        public override void DisposeScope()
        {
            scope.Dispose();
        }
    }
}

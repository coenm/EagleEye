namespace EagleEye.Photo.ReadModel.Similarity.Internal.SimpleInjectorAdapter
{
    using System;

    using Hangfire;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;
    using SimpleInjector;

    /// <remarks>Based on <see href="https://github.com/devmondo/HangFire.SimpleInjector/blob/master/src/HangFire.SimpleInjector/SimpleInjectorJobActivator.cs"/>. (accessed at 2018-11-28).</remarks>
    internal class SimpleInjectorJobActivator : JobActivator
    {
        private readonly Container container;

        public SimpleInjectorJobActivator([NotNull] Container container)
        {
            Helpers.Guards.Guard.NotNull(container, nameof(container));
            this.container = container;
        }

        public override object ActivateJob(Type jobType)
        {
            return container.GetInstance(jobType);
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new SimpleInjectorAsyncLifestyleScope(container);
        }
    }
}

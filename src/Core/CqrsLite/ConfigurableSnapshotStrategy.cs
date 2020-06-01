namespace EagleEye.Core.CqrsLite
{
    using System;
    using System.Reflection;

    using CQRSlite.Domain;
    using CQRSlite.Snapshotting;
    using Dawn;
    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// Implementation of snapshot strategy. SnapshotInterval is configurable.
    /// </summary>
    public class ConfigurableSnapshotStrategy : ISnapshotStrategy
    {
        private readonly uint snapshotInterval;

        public ConfigurableSnapshotStrategy(ushort snapshotInterval)
        {
            Guard.Argument(snapshotInterval, nameof(snapshotInterval)).NotNegative().NotZero();
            this.snapshotInterval = snapshotInterval;
        }

        public bool IsSnapshotable([NotNull] Type aggregateType)
        {
            Guard.Argument(aggregateType, nameof(aggregateType)).NotNull();

            var baseType = aggregateType.GetTypeInfo().BaseType;

            if (baseType == null)
                return false;
            if (baseType.GetTypeInfo().IsGenericType && baseType.GetGenericTypeDefinition() == typeof(SnapshotAggregateRoot<>))
                return true;
            return IsSnapshotable(baseType);
        }

        public bool ShouldMakeSnapShot(AggregateRoot aggregate)
        {
            if (aggregate == null)
                return false;

            if (!IsSnapshotable(aggregate.GetType()))
                return false;

            int uncommittedChangesCount = aggregate.GetUncommittedChanges().Length;

            if (uncommittedChangesCount == 0)
                return false;

            if (uncommittedChangesCount >= snapshotInterval)
                return true;

            var aggregateVersion = aggregate.Version;
            return (aggregateVersion % snapshotInterval) + uncommittedChangesCount >= snapshotInterval;
        }
    }
}

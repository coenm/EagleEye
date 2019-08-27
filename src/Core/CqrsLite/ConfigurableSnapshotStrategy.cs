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

            if (aggregateType.GetTypeInfo().BaseType == null)
                return false;
            if (aggregateType.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                aggregateType.GetTypeInfo().BaseType.GetGenericTypeDefinition() == typeof(SnapshotAggregateRoot<>))
                return true;
            return IsSnapshotable(aggregateType.GetTypeInfo().BaseType);
        }

        public bool ShouldMakeSnapShot(AggregateRoot aggregate)
        {
            if (!IsSnapshotable(aggregate.GetType()))
                return false;

            var i = aggregate.Version;
            for (var j = 0; j < aggregate.GetUncommittedChanges().Length; j++)
            {
                if (++i % snapshotInterval == 0 && i != 0)
                    return true;
            }

            return false;
        }
    }
}

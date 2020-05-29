namespace EagleEye.Core.DefaultImplementations
{
    using System;

    using EagleEye.Core.Interfaces.Core;

    public sealed class SystemDateTimeService : IDateTimeService
    {
        private SystemDateTimeService()
        {
        }

        public static SystemDateTimeService Instance { get; } = new SystemDateTimeService();

        public DateTime Now => DateTime.Now;

        public DateTime Today => DateTime.Today;

        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}

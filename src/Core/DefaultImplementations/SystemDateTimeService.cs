namespace EagleEye.Core.DefaultImplementations
{
    using System;

    using EagleEye.Core.Interfaces;

    public class SystemDateTimeService : IDateTimeService
    {
        private SystemDateTimeService()
        {
        }

        public SystemDateTimeService Instance { get; } = new SystemDateTimeService();

        public DateTime Now => DateTime.Now;

        public DateTime Today => DateTime.Today;
    }
}
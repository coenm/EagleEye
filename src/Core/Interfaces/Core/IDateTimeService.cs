namespace EagleEye.Core.Interfaces.Core
{
    using System;

    public interface IDateTimeService
    {
        DateTime Now { get; }

        DateTime Today { get; }

        /// <summary>
        /// Retrieves the current system time in UTC.
        /// </summary>
        DateTimeOffset UtcNow { get; }
    }
}

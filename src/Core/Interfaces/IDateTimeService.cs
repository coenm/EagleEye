namespace EagleEye.Core.Interfaces
{
    using System;

    public interface IDateTimeService
    {
        DateTime Now { get; }

        DateTime Today { get; }
    }
}

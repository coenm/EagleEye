namespace EagleEye.Core.Interfaces.Core
{
    using System;

    public interface IDateTimeService
    {
        DateTime Now { get; }

        DateTime Today { get; }
    }
}

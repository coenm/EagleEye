namespace EagleEye.Core.Interfaces.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;

    public interface IEventExporter
    {
        /// <summary>
        /// Receive all events happened after <paramref name="from"/>.
        /// </summary>
        /// <param name="from">DateTime to mark start of events period.</param>
        /// <param name="cancellationToken">Token to cancel operation.</param>
        /// <returns>All events happened after <paramref name="from"/>.</returns>
        Task<IEnumerable<IEvent>> GetAsync(DateTime from, CancellationToken cancellationToken = default);
    }
}

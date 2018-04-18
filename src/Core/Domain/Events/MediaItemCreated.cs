namespace EagleEye.Core.Domain.Events
{
    using System;
    using System.Diagnostics;

    using CQRSlite.Events;

    public class MediaItemCreated : IEvent
    {
        public readonly string Name;

        [DebuggerStepThrough]
        public MediaItemCreated(Guid id, string name, string[] tags, string[] persons )
        {
            Id = id;
            Tags = tags ?? new string[0];
            Persons = persons ?? new string[0];
            Name = name;
        }

        public Guid Id { get; set; }

        public string[] Tags { get; }

        public string[] Persons { get; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}
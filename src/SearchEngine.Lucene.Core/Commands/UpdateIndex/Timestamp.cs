namespace SearchEngine.LuceneNet.Core.Commands.UpdateIndex
{
    using System;

    public class Timestamp
    {
        public DateTime Value { get; set; }

        public TimestampPrecision Precision { get; set; }
    }
}
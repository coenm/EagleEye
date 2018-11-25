namespace SearchEngine.Interface.Commands.ParameterObjects
{
    using System;

    public class Timestamp
    {
        public DateTime Value { get; set; }

        public TimestampPrecision Precision { get; set; }
    }
}

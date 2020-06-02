namespace EagleEye.Core.Data
{
    using JetBrains.Annotations;

    public class MediaObject
    {
        [CanBeNull]
        public Timestamp DateTimeTaken { get; private set; }

        public void SetDateTimeTaken([NotNull] Timestamp value)
        {
            DateTimeTaken = value;
        }
    }
}

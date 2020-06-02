namespace EagleEye.Core.Data
{
    using Dawn;
    using JetBrains.Annotations;

    public class MediaObject
    {
     public MediaObject(string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotWhiteSpace();

            FileInformation = new FileInformation(filename);
            Location = new Location();
        }

        public FileInformation FileInformation { get; }

        public Location Location { get; }

        [CanBeNull]
        public Timestamp DateTimeTaken { get; private set; }

        public void SetDateTimeTaken([NotNull] Timestamp value)
        {
            DateTimeTaken = value;
        }
    }
}

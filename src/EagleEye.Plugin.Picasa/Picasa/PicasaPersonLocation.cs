namespace EagleEye.Picasa.Picasa
{
    using System;

    using Dawn;
    using JetBrains.Annotations;

    public class PicasaPersonLocation : IEquatable<PicasaPersonLocation>
    {
        public PicasaPersonLocation([NotNull] string name, [CanBeNull] RelativeRegion? region = null)
        {
            Guard.Argument(name, nameof(name)).NotNull().NotWhiteSpace();
            Name = name;
            Region = region;
        }

        public string Name { get; }

        [CanBeNull]
        public RelativeRegion? Region { get; }

        public bool Equals(PicasaPersonLocation other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Name == other.Name
                   &&
                   Nullable.Equals(Region, other.Region);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;

            return Equals((PicasaPersonLocation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ Region.GetHashCode();
            }
        }
    }
}

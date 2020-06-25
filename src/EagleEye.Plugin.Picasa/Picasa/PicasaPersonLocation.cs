namespace EagleEye.Picasa.Picasa
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    using JetBrains.Annotations;

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class PicasaPersonLocation : IEquatable<PicasaPersonLocation>, ICloneable
    {
        public PicasaPersonLocation([NotNull] PicasaPerson person, [CanBeNull] Rect64RelativeRegion? region = null)
        {
            Person = person;
            Region = region;
        }

        public PicasaPersonLocation([NotNull] string person, [CanBeNull] Rect64RelativeRegion? region = null)
        {
            Person = new PicasaPerson(string.Empty, person);
            Region = region;
        }

        [NotNull]
        public PicasaPerson Person { get; private set; }

        [CanBeNull]
        public Rect64RelativeRegion? Region { get; }

        [DebuggerNonUserCode]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "DebuggerDisplay")]
        private string DebuggerDisplay => ToString();

        public override string ToString()
        {
            if (Region == null)
                return Person + " [Region unknown]";
            return Person + "; Region:" + Region.Value.Rect64;
        }

        public void UpdatePerson([NotNull] PicasaPerson picasaPerson)
        {
            if (string.IsNullOrWhiteSpace(picasaPerson.Name))
                throw new ArgumentNullException(nameof(picasaPerson.Name));

            Person = picasaPerson;
        }

        public bool Equals(PicasaPersonLocation other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Person.Equals(other.Person) && Nullable.Equals(Region, other.Region);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((PicasaPersonLocation)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Person.GetHashCode() * 397) ^ Region.GetHashCode();
            }
        }

        public object Clone()
        {
            return new PicasaPersonLocation(Person, Region);
        }
    }
}

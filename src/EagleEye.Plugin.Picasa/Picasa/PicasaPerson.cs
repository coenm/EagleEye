namespace EagleEye.Picasa.Picasa
{
    using System;

    using Dawn;
    using JetBrains.Annotations;

    public class PicasaPerson : IEquatable<PicasaPerson>
    {
        public PicasaPerson([NotNull] string name)
        {
            Guard.Argument(name, nameof(name)).NotNull().NotWhiteSpace();
            Name = name;
        }

        public string Name { get; }

        public bool Equals(PicasaPerson other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((PicasaPerson) obj);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }
    }
}

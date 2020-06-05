namespace EagleEye.Picasa.Picasa
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    using Dawn;
    using JetBrains.Annotations;

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public readonly struct PicasaPerson : IEquatable<PicasaPerson>
    {
        public PicasaPerson([NotNull] string id, [NotNull] string name)
        {
            Guard.Argument(name, nameof(name)).NotNull(); // can be empty
            Guard.Argument(id, nameof(id)).NotNull(); // can be empty
            Id = id;
            Name = name;
        }

        public string Id { get; }

        public string Name { get; }

        public bool Equals(PicasaPerson other)
        {
            return Id == other.Id && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is PicasaPerson other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Id)
                       ? $"{Name} (<empty id>)"
                       : $"{Name} ({Id})";
        }

        [DebuggerNonUserCode]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "DebuggerDisplay")]
        private string DebuggerDisplay => ToString();
    }
}

namespace EagleEye.Picasa.Picasa
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    using Dawn;
    using JetBrains.Annotations;

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public readonly struct PicasaContact : IEquatable<PicasaContact>
    {
        public PicasaContact([JetBrains.Annotations.NotNull] string id, [JetBrains.Annotations.NotNull] string name, [CanBeNull] string displayValue, [CanBeNull] string modifiedTimeValue, [CanBeNull] string localContactValue)
        {
            Guard.Argument(name, nameof(name)).NotNull(); // can be empty
            Guard.Argument(id, nameof(id)).NotNull(); // can be empty
            Id = id;
            Name = name;
            DisplayName = displayValue;
            ModifiedDateTime = modifiedTimeValue;
            IsLocalContact = localContactValue;
        }

        public string Id { get; }

        public string Name { get; }

        public string DisplayName { get; }

        public string ModifiedDateTime { get; }

        public string IsLocalContact { get; }

        [DebuggerNonUserCode]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "DebuggerDisplay")]
        private string DebuggerDisplay => ToString();

        public bool Equals(PicasaContact other)
        {
            return Id == other.Id && Name == other.Name && DisplayName == other.DisplayName && ModifiedDateTime == other.ModifiedDateTime && IsLocalContact == other.IsLocalContact;
        }

        public override bool Equals(object obj)
        {
            return obj is PicasaContact other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id != null ? Id.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ModifiedDateTime != null ? ModifiedDateTime.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (IsLocalContact != null ? IsLocalContact.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Id)
                       ? $"{Name} (<empty id>)"
                       : $"{Name} ({Id})";
        }
    }
}

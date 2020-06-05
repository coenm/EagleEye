namespace EagleEye.Picasa.Picasa
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dawn;

    public class PicasaIniFile : IEquatable<PicasaIniFile>, ICloneable
    {
        public PicasaIniFile(List<FileWithPersons> files, List<PicasaPerson> persons)
        {
            Files = files ?? new List<FileWithPersons>();
            Persons = persons ?? new List<PicasaPerson>();
        }

        public PicasaIniFile(PicasaIniFile original)
        {
            Guard.Argument(original, nameof(original)).NotNull();
            Files = original.Files.ToList();
            Persons = original.Persons.ToList();
        }

        public List<FileWithPersons> Files { get; }

        public List<PicasaPerson> Persons { get; }

        public object Clone()
        {
            return new PicasaIniFile(this);
        }

        public bool Equals(PicasaIniFile other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return (Equals(Files, other.Files) || Files.SequenceEqual(other.Files))
                   &&
                   (Equals(Persons, other.Persons) || Persons.SequenceEqual(other.Persons));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((PicasaIniFile)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Files != null ? Files.GetHashCode() : 0) * 397) ^ (Persons != null ? Persons.GetHashCode() : 0);
            }
        }
    }
}

namespace EagleEye.Picasa.Picasa
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class FileWithPersons : IEquatable<FileWithPersons>
    {
        private readonly List<PicasaPersonLocation> persons;

        public FileWithPersons(string filename, params PicasaPersonLocation[] persons)
        {
            Filename = filename;
            this.persons = persons.ToList();
        }

        public string Filename { get; }

        public List<PicasaPersonLocation> Persons => persons;

        public void AddPerson(PicasaPersonLocation person)
        {
            if (persons.Contains(person))
                return;

            persons.Add(person);
        }

        public override string ToString()
        {
            var result = $"{Filename} has ";
            if (!Persons.Any())
                return result + "no persons.";

            result += "persons:";
            result = Persons.Aggregate(result, (current, person) => current + " " + person.Person.Name + ",");
            return result.Substring(0, result.Length - 1);
        }

        [DebuggerNonUserCode]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "DebuggerDisplay")]
        private string DebuggerDisplay => ToString();

        public bool Equals(FileWithPersons other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return (Equals(persons, other.persons) || persons.SequenceEqual(other.persons))
                   &&
                   Filename == other.Filename;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((FileWithPersons)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((persons != null ? persons.GetHashCode() : 0) * 397) ^ (Filename != null ? Filename.GetHashCode() : 0);
            }
        }
    }
}

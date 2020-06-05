namespace EagleEye.Picasa.Picasa
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class FileWithPersons
    {
        private readonly List<PicasaPersonLocation> persons;

        public FileWithPersons(string filename, params PicasaPersonLocation[] persons)
        {
            Filename = filename;
            this.persons = persons.ToList();
        }

        public string Filename { get; }

        public IEnumerable<PicasaPersonLocation> Persons => persons.AsReadOnly();

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
    }
}

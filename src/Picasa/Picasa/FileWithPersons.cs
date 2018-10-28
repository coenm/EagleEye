namespace EagleEye.Picasa.Picasa
{
    using System.Collections.Generic;
    using System.Linq;

    public class FileWithPersons
    {
        private readonly List<string> persons;

        public FileWithPersons(string filename, params string[] persons)
        {
            Filename = filename;
            this.persons = persons.ToList();
        }

        public string Filename { get; }

        public IEnumerable<string> Persons => persons.AsReadOnly();

        public void AddPerson(string person)
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
            result = Persons.Aggregate(result, (current, person) => current + " " + person + ",");
            return result.Substring(0, result.Length - 1);
        }
    }
}
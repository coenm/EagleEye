namespace EagleEye.Core
{
    using System.Collections.Generic;

    using Helpers.Guards;
    using JetBrains.Annotations;

    public class MediaObject
    {
        private readonly List<string> persons;
        private readonly List<string> tags;

        public MediaObject(string filename)
        {
            Guard.NotNullOrWhiteSpace(filename, nameof(filename));

            FileInformation = new FileInformation(filename);
            Location = new Location();
            persons = new List<string>();
            tags = new List<string>();
        }

        public FileInformation FileInformation { get; }

        public IReadOnlyList<string> Persons => persons;

        public IReadOnlyList<string> Tags => tags;

        public Location Location { get; }

        [CanBeNull]
        public Timestamp DateTimeTaken { get; private set; }

        public void SetDateTimeTaken([NotNull] Timestamp value)
        {
            DateTimeTaken = value;
        }

        public void ClearDateTimeTaken()
        {
            DateTimeTaken = null;
        }

        public void AddPersons([NotNull] IEnumerable<string> persons)
        {
            Guard.NotNull(persons, nameof(persons));

            foreach (var person in persons)
                AddPerson(person);
        }

        public void AddPerson(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            name = name.Trim();

            if (tags.Contains(name))
                return;

            persons.Add(name);
        }

        public void AddTags([NotNull] IEnumerable<string> tags)
        {
            Guard.NotNull(tags, nameof(tags));

            foreach (var tag in tags)
                AddTag(tag);
        }

        public void AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            tag = tag.Trim();

            if (tags.Contains(tag))
                return;

            tags.Add(tag);
        }
    }
}
